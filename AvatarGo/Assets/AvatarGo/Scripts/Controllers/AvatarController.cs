using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{
    public AvatarDriver driver;
    public AvatarBody body;
    public bool ikActive = false;
}
