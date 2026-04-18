using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using System.Linq;

namespace StaleBodySweeper
{
    // Custom component to track how long the warning has been active
    public struct StaleCounter : IComponentData
    {
        public uint m_GraceTimer;
    }

    public partial class StaleBodySweeperSystem : SystemBase
    {
        private int m_abandonedBodyCount = 0;
        private const int CheckIntervalFrames = 1000; // Check frames interval

        protected override void OnCreate()
        {
            base.OnCreate();
            m_abandonedBodyCount = Mod.m_Setting.abandonedBodyCount;
            Mod.log.Info("StaleBodySweeperSystem created.");
        }

        protected override void OnUpdate()
        {
            // Execute every 256 frames (approx. every few seconds)
            if (UnityEngine.Time.frameCount % CheckIntervalFrames != 0) return;

            EntityQuery deadQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Citizen>(),
                    ComponentType.ReadWrite<HealthProblem>()
                },
                None = new ComponentType[] { ComponentType.ReadOnly<Deleted>() }
            });

            if (deadQuery.IsEmptyIgnoreFilter) return;

            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            // The limit observed in the game code/behavior
            uint gameLimit = 35;

            int removedCount = 0;
            int trackingCount = 0;
            Dictionary<uint, int> timerStats = new Dictionary<uint, int>();

            var entities = deadQuery.ToEntityArray(Allocator.Temp);
            var healthProblems = deadQuery.ToComponentDataArray<HealthProblem>(Allocator.Temp);
            // Lookup for our custom counter
            var staleCounters = GetComponentLookup<StaleCounter>(false);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity citizen = entities[i];
                HealthProblem hp = healthProblems[i];

                if ((hp.m_Flags & HealthProblemFlags.Dead) != 0)
                {
                    uint currentTimer = hp.m_Timer;

                    // Stats collection
                    if (timerStats.ContainsKey(currentTimer))
                        timerStats[currentTimer]++;
                    else
                        timerStats[currentTimer] = 1;

                    // Logic for citizens at the limit
                    if (currentTimer >= gameLimit)
                    {
                        if (staleCounters.HasComponent(citizen))
                        {
                            // Already tracking: increment the grace timer
                            StaleCounter sc = staleCounters[citizen];
                            sc.m_GraceTimer++;

                            if (sc.m_GraceTimer >= m_abandonedBodyCount)
                            {
                                // Grace period expired: Remove
                                commandBuffer.RemoveComponent<HealthProblem>(citizen);
                                commandBuffer.AddComponent<Deleted>(citizen);
                                removedCount++;
                            }
                            else
                            {
                                // Update the counter
                                commandBuffer.SetComponent(citizen, sc);
                                trackingCount++;
                            }
                        }
                        else
                        {
                            // Reached limit for the first time: Start tracking
                            commandBuffer.AddComponent(citizen, new StaleCounter { m_GraceTimer = 0 });
                            trackingCount++;
                        }
                    }
                    else if (staleCounters.HasComponent(citizen))
                    {
                        // If timer dropped below limit (e.g., picked up), stop tracking
                        commandBuffer.RemoveComponent<StaleCounter>(citizen);
                    }
                }
            }

            // --- Log Statistics ---
            string statsOutput = $"--- Dead Citizen Report ---\n";
            var sortedStats = timerStats.OrderBy(x => x.Key);
            foreach (var stat in sortedStats)
            {
                statsOutput += $"Timer [{stat.Key}]: {stat.Value} citizens\n";
            }
            statsOutput += $"--------------------------------\n";
            statsOutput += $"Tracking(Grace): {trackingCount} | Removed: {removedCount} (Grace Limit: {m_abandonedBodyCount})";

            Mod.log.Info(statsOutput);

            entities.Dispose();
            healthProblems.Dispose();
            commandBuffer.Playback(EntityManager);
            commandBuffer.Dispose();
        }
    }
}