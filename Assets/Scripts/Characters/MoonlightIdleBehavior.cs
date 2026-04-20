using System.Collections;
using UnityEngine;

namespace MoonlightMagicHouse
{
    // Moonlight wanders gently around the active room when idle.
    // She pauses, looks around, then moves to a new spot.
    public class MoonlightIdleBehavior : MonoBehaviour
    {
        [SerializeField] float moveSpeed    = 0.8f;
        [SerializeField] float wanderRadius = 1.8f;
        [SerializeField] float pauseMin     = 2f;
        [SerializeField] float pauseMax     = 5f;

        MoonlightAnimator _anim;
        Vector3           _origin;
        bool              _moving;

        void Start()
        {
            _anim   = GetComponent<MoonlightAnimator>();
            _origin = transform.position;
            StartCoroutine(WanderLoop());
        }

        IEnumerator WanderLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(pauseMin, pauseMax));
                var target = _origin + new Vector3(
                    Random.Range(-wanderRadius, wanderRadius), 0,
                    Random.Range(-wanderRadius, wanderRadius));
                yield return MoveTo(target);
            }
        }

        IEnumerator MoveTo(Vector3 target)
        {
            _moving = true;
            _anim?.SetWalking(true);
            // Face direction
            var dir = (target - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            _moving = false;
            _anim?.SetWalking(false);
        }

        // Called when player interacts — pause wandering briefly
        public void PauseWander(float seconds)
        {
            StopAllCoroutines();
            _anim?.SetWalking(false);
            StartCoroutine(ResumeAfter(seconds));
        }

        IEnumerator ResumeAfter(float s)
        {
            yield return new WaitForSeconds(s);
            StartCoroutine(WanderLoop());
        }
    }
}
