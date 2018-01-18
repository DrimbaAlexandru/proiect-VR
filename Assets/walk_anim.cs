using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class walk_anim : MonoBehaviour {
    public Animation anim;
    public AnimationClip clip;
    void Start()
    {
        anim = GetComponent<Animation>();
        Transform[] all = transform.GetComponentsInChildren<Transform>();
        Transform[] shldr = new Transform[4];
        int k = 0;
        for (int i = 0; i < all.Length - 1; i++)
        {
            if (all[i].name == "lShldrBend" || all[i].name == "rShldrBend")
            {
                shldr[k] = all[i];
                Debug.Log(shldr[k].name);
                k++;
            }
        }
        shldr[0].localRotation = new Quaternion(0, 0, 1, 1);
        shldr[1].localRotation = new Quaternion(0, 0, -1, 1);

        clip = new AnimationClip();
        clip.legacy = true;
        AnimationCurve curve;
        putAllTogether();
        anim.AddClip(clip, "asdgahvc");
        //M3DMale/hip/abdomenLower/abdomenUpper/chestLower/chestUpper/lCollar/lShldrBend
    }

    void moveComponent(string path, Vector3 rotation, Vector3 init, float angle, float phase)
    {
        Quaternion rot; //Quaternion we'll be storing the rotation in
        //float angle = 60; //rotation each keyframe
        float time = 0.05f; //time between keyframes
        Vector3 axis = rotation; //define the axis of rotation
        int keyframes = 24; //how many keys to add

        //the four curves; one for each quaternion property
        AnimationCurve xCurve = new AnimationCurve(), yCurve = new AnimationCurve(), zCurve = new AnimationCurve(), wCurve = new AnimationCurve();

        for (int k = 0; k <= keyframes; k++)
        {
            rot = Quaternion.AngleAxis(angle * Mathf.Sin((float)k/keyframes*Mathf.PI*2+phase/360*Mathf.PI*2), axis) * Quaternion.Euler(init); //create our quaternion key for this keyframe

            //create the keys
            xCurve.AddKey(time * k, rot.x);
            yCurve.AddKey(time * k, rot.y);
            zCurve.AddKey(time * k, rot.z);
            wCurve.AddKey(time * k, rot.w);
            //xCurve.AddKey(time * (k + 0.03f), -rot.x);
            //yCurve.AddKey(time * (k + 0.03f), -rot.y);
            //zCurve.AddKey(time * (k + 0.03f), -rot.z);
            //wCurve.AddKey(time * (k + 0.03f), rot.w);
        }

        //set the curves on the clip  
        clip.SetCurve(path, typeof(Transform), "localRotation.x", xCurve);
        clip.SetCurve(path, typeof(Transform), "localRotation.y", yCurve);
        clip.SetCurve(path, typeof(Transform), "localRotation.z", zCurve);
        clip.SetCurve(path, typeof(Transform), "localRotation.w", wCurve);
    }

    void moveSoulderLeft()
    {
        this.moveComponent("M3DMale/hip/abdomenLower/abdomenUpper/chestLower/chestUpper/lCollar/lShldrBend", new Vector3(1, 0, 0), new Vector3(-5, 0, 80), 15, -15);
    }

    void moveShoulderRight()
    {
        this.moveComponent("M3DMale/hip/abdomenLower/abdomenUpper/chestLower/chestUpper/rCollar/rShldrBend", new Vector3(-1, 0, 0), new Vector3(-5, 0, -80), 15, -15);
    }

    void moveThighL()
    {
        this.moveComponent("M3DMale/hip/pelvis/lThighBend", new Vector3(-1, 0, 0), new Vector3(-5, 0, 0), 20, 0);
    }

    void moveThighR()
    {
        this.moveComponent("M3DMale/hip/pelvis/rThighBend", new Vector3(1, 0, 0), new Vector3(-5, 0, 0), 20, 0);
    }

    void moveElbowLeft()
    {
        this.moveComponent("M3DMale/hip/abdomenLower/abdomenUpper/chestLower/chestUpper/lCollar/lShldrBend/lShldrTwist/lForearmBend", new Vector3(1, 0, 0), new Vector3(-5, 0, 0), 15, -15);
        //this.moveComponent("M3DMale/hip/abdomenLower/abdomenUpper/chestLower/chestUpper/lCollar/lShldrBend/lShldrTwist/lForearmBend", new Vector3(1, 0, 0), new Vector3(-5, 0, -80), 15, -15);
    }

    void moveElbowRight()
    {
        this.moveComponent("M3DMale/hip/abdomenLower/abdomenUpper/chestLower/chestUpper/rCollar/rShldrBend/rShldrTwist/rForearmBend", new Vector3(-1, 0, 0), new Vector3(-5, 0, 0), 15, -15);
        //this.moveComponent("M3DMale/hip/abdomenLower/abdomenUpper/chestLower/chestUpper/rCollar/rShldrBend/rShldrTwist/rForearmBend", new Vector3(-1, 0, 0), new Vector3(-5, 0, 80), 15, -15);
    }

    void moveKneeL()
    {
        this.moveComponent("M3DMale/hip/pelvis/lThighBend/lThighTwist/lShin", new Vector3(-1, 0, 0), new Vector3(5, 0, 0), 15, -90);
        //this.moveComponent("M3DMale/hip/pelvis/lThighBend/lThighTwist/lShin", new Vector3(1, 0, 0), new Vector3(-5, 0, 0), 20, 0);
    }

    void moveKneeR()
    {
        this.moveComponent("M3DMale/hip/pelvis/rThighBend/rThighTwist/rShin", new Vector3(1, 0, 0), new Vector3(5, 0, 0), 15, -90);
        //this.moveComponent("M3DMale/hip/pelvis/rThighBend/rThighTwist/rShin", new Vector3(-1, 0, 0), new Vector3(-5, 0, 0), 20, 0);
    }

    void moveWaist()
    {
        this.moveComponent("M3DMale/hip/abdomenLower", new Vector3(0, -1, 0), new Vector3(0, 0, 0), 2, 0);
        this.moveComponent("M3DMale/hip/abdomenLower", new Vector3(0, 0, 1), new Vector3(0, 0, 0), 2, 0);
    }

    void putAllTogether()
    {
        this.moveSoulderLeft();
        this.moveShoulderRight();
        this.moveThighL();
        this.moveThighR();
        this.moveElbowLeft();
        this.moveElbowRight();
        this.moveKneeL();
        this.moveKneeR();
        this.moveWaist();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("W");
            anim.Play("asdgahvc");
        }
        else
            if (Input.GetKey(KeyCode.S) || Input.GetKeyUp(KeyCode.W))
            {
            Debug.Log("S");
            anim.Stop("asdgahvc");
        }
        else
            if (Input.GetKey(KeyCode.A))
            {
            Debug.Log("A");
            anim.Stop("asdgahvc");
        }
        else
            if (Input.GetKey(KeyCode.D))
            {
            Debug.Log("D");
            anim.Play("asdgahvc");
        }
        /*
        if (Input.GetMouseButtonDown(0))
            anim.Play("asdgahvc");
        else
            anim.Stop("asdgahvc");
            */
    }
}
