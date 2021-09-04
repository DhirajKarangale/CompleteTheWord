using Photon.Pun;
using UnityEngine;

public class PowerEffect : MonoBehaviour
{
    public PhotonView photonView;
    [SerializeField] Animator animator;
    public Wall wall;


    private int sleepTime = 10;
    public bool isSleep;

    [PunRPC]
    public void HammerCollide()
    {
        if (!photonView.IsMine) return;
        if (isSleep) return;
        if (wall != null)
        {
            if (wall.isWallUp) return;
        }

        isSleep = true;

        animator.SetBool("isRun", false);
        animator.SetBool("isWin", false);
        animator.SetBool("isLoose", true);
        animator.SetBool("isJump", false);

        Invoke("SetSleepFalse", sleepTime);
    }

    private void SetSleepFalse()
    {
        isSleep = false;
    }
}
