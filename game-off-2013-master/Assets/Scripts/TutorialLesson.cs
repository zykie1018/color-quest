using UnityEngine;
using System.Collections;

public class TutorialLesson : MonoBehaviour
{
	public Lesson lesson;

	public enum Lesson {
		Laser,
		Shields,
		Slow
	}
	
	// Update is called once per frame
	void OnTriggerEnter (Collider other) {
		if (other.CompareTag (Tags.PLAYER)) {
			if (GameManager.Instance.player.IsDead) {
				return;
			}
			// Player crossed the line without being dead, mark it done
			GameManager.Instance.MarkLessonComplete (lesson);
			if (lesson == Lesson.Slow) {
				GameManager.Instance.MarkTutorialComplete ();
				GameManager.Instance.treadmill.ResetTreadmill ();
			}
		}
	}
}