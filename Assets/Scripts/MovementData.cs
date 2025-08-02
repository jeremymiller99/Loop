using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MovementFrame
{
    public float timestamp;
    public Vector3 position;
    public Vector3 velocity;
    public bool isGrounded;
    public float horizontalInput;
    public bool jumpInput;
    public bool shootInput; // For Player 2
    public Vector3 mousePosition; // For Player 2 shooting direction
    public bool facingRight;

    public MovementFrame(float time, Vector3 pos, Vector3 vel, bool grounded, float hInput, bool jInput, bool sInput = false, Vector3 mousePos = default, bool facing = true)
    {
        timestamp = time;
        position = pos;
        velocity = vel;
        isGrounded = grounded;
        horizontalInput = hInput;
        jumpInput = jInput;
        shootInput = sInput;
        mousePosition = mousePos;
        facingRight = facing;
    }

    public MovementFrame() { }
}

[System.Serializable]
public class MovementRecording
{
    public List<MovementFrame> frames = new List<MovementFrame>();
    public float duration;
    public Vector3 startPosition;
    public bool isComplete;

    public MovementRecording()
    {
        frames = new List<MovementFrame>();
        duration = 0f;
        isComplete = false;
    }

    public void AddFrame(MovementFrame frame)
    {
        frames.Add(frame);
        duration = frame.timestamp;
    }

    public void Clear()
    {
        frames.Clear();
        duration = 0f;
        isComplete = false;
    }

    public MovementFrame GetFrameAtTime(float time)
    {
        if (frames.Count == 0) return null;

        // Handle time beyond recording
        if (time >= duration)
        {
            return frames[frames.Count - 1];
        }

        // Find the appropriate frame
        for (int i = 0; i < frames.Count - 1; i++)
        {
            if (time >= frames[i].timestamp && time < frames[i + 1].timestamp)
            {
                // Interpolate between frames if needed
                float t = (time - frames[i].timestamp) / (frames[i + 1].timestamp - frames[i].timestamp);
                return InterpolateFrames(frames[i], frames[i + 1], t);
            }
        }

        // Return first frame if time is before recording started
        return frames[0];
    }

    private MovementFrame InterpolateFrames(MovementFrame frameA, MovementFrame frameB, float t)
    {
        MovementFrame interpolated = new MovementFrame();
        interpolated.timestamp = Mathf.Lerp(frameA.timestamp, frameB.timestamp, t);
        interpolated.position = Vector3.Lerp(frameA.position, frameB.position, t);
        interpolated.velocity = Vector3.Lerp(frameA.velocity, frameB.velocity, t);
        
        // For boolean values, use the first frame's values (no interpolation needed)
        interpolated.isGrounded = frameA.isGrounded;
        interpolated.horizontalInput = frameA.horizontalInput;
        interpolated.jumpInput = frameA.jumpInput;
        interpolated.shootInput = frameA.shootInput;
        interpolated.mousePosition = Vector3.Lerp(frameA.mousePosition, frameB.mousePosition, t);
        interpolated.facingRight = frameA.facingRight;

        return interpolated;
    }
}