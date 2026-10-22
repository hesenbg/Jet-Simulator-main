using UnityEngine;
using System;
public struct SODState
{
    public Vector3 xp;      // previous input
    public Vector3 y;       // output position
    public Vector3 yd;      // output velocity
    public float k1, k2, k3;
}
public static class SOD
{


    public static SODState SODCreate(float f, float z, float r, Vector3 x0)
    {
        SODState s;
        s.k1 = z / (Mathf.PI * f);
        s.k2 = 1f / ((2f * Mathf.PI * f) * (2f * Mathf.PI * f));
        s.k3 = r * z / (2f * Mathf.PI * f);
        s.xp = x0;
        s.y = x0;
        s.yd = Vector3.zero;
        return s;
    }

    public static Vector3 SODUpdate(ref SODState state, float T, Vector3 x, Vector3? xd = null)
    {
        Vector3 vel = xd ?? (x - state.xp) / T;
        if (xd == null) state.xp = x;

        float k2Stable = Mathf.Max(state.k2,
                        Mathf.Max(T * T / 2f + T * state.k1 / 2f,
                        T * state.k1));

        state.y += T * state.yd;
        state.yd += T * (x + state.k3 * vel - state.y - state.k1 * state.yd) / k2Stable;
        return state.y;
    }
}