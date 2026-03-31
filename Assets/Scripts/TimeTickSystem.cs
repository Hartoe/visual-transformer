using System;
using UnityEngine;

public class TimeTickSystem : MonoBehaviour
{
    public class TickEventArgs : EventArgs
    {
        public int tick;
    }
    public static event EventHandler<TickEventArgs> OnTick;
    public static bool DoTimeTick { get; private set; }
    public static void SetTimeTick(bool value) => DoTimeTick = value;

    private const float TICK_TIMER_MAX = 1f;
    public static int tick;
    private float tickTimer;

    void Awake()
    {
        DoTimeTick = true;
        tick = 0;
    }

    void Update()
    {
        if (DoTimeTick)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= TICK_TIMER_MAX)
            {
                tickTimer -= TICK_TIMER_MAX;
                tick++;
                if (OnTick != null) OnTick(this, new TickEventArgs{tick = tick});
            }
        }
    }
}
