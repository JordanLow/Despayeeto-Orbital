using UnityEngine;

public class RadicalAI : EnemyAI
{
	private bool launching = false;
	[SerializeField] ParticleSystem particles;
	
	protected override void FixedUpdate() {
		if (launching == true) {
			Vector3 dir = Vector3.zero;
			Collider[] hits = Physics.OverlapSphere(transform.position, 4f, LayerMask.GetMask("Player"));
			if (hits.Length > 0) {
				dir = hits[0].transform.position - transform.position;
				dir.y += 0.7f;
				dir = dir.magnitude > Time.fixedDeltaTime * 3f ? dir.normalized * Time.fixedDeltaTime * 3f: dir;
			} else {
				dir.y = Time.fixedDeltaTime * 3f;
			}
			rb.MovePosition(transform.position + dir);
		} else {
			base.FixedUpdate();
		}
	}
	
	public void RadicalLaunch() {
		launching = true;
		Destroy(GetComponent<Collider>());
	}
	
	public void ExplosionBegins() {
		particles.Play();
		SkinnedMeshRenderer[] skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var smr in skinnedMeshes)
        {
            if (smr.gameObject != gameObject)
            {
                Destroy(smr);
            }
        }
		launching = false;
	}
	
    public override void AnimationEndHelper() {
		Destroy(gameObject);
	}
}
