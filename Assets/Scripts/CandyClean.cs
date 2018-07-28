using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyClean : MonoBehaviour {
	public AnimationClip clearAnimation;

	private bool isClearing;


	public bool IsClearing
	{
		get
		{
			return isClearing;
		}
	}

	public virtual void Cleanup(float time)
	{
		isClearing = true;
		StartCoroutine(ClearCoroutine(time));
	}

	private IEnumerator ClearCoroutine(float time)
	{
		yield return new WaitForSeconds(time * 3);
		Animator animator = GetComponent<Animator>();

		if (animator!=null)
		{
			animator.Play(clearAnimation.name);
			//玩家得分+1 播放清楚声音
//			GameManager.Instance.playerScore++;
//			AudioSource.PlayClipAtPoint(destoryAudio, transform.position);
			yield return new WaitForSeconds(clearAnimation.length);
			Destroy(gameObject);

		}
	}
}
