using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTickSystem : MonoBehaviour
{
    public class TickEventArgs : EventArgs
    {
        public int tick;
    }
    public static event EventHandler<TickEventArgs> OnTick;

    private const float TICK_TIMER_MAX = 1f;
    private int tick;
    private float tickTimer;

    void Awake()
    {
        tick = 0;
    }

    void Update()
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
