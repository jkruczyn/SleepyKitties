using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Animator anim;
	public bool isSleeping = false;

	private Vector2 lastMove = new Vector2(0,0);
	private void Awake() {
	}
	// Start is called before the first frame update
	void Start()
    {
    }

    // Update is called once per frame
    void Update() {



		// Movement 
		if (isSleeping == false) {
			if (Input.GetKeyDown(KeyCode.LeftArrow)) {
				lastMove = new Vector2(-1, 0);
				Move(lastMove);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow)) {
				lastMove = new Vector2(1, 0);
				Move(lastMove);
			}
			if (Input.GetKeyDown(KeyCode.UpArrow)) {
				lastMove = new Vector2(0, 1);
				Move(lastMove);
			}
			if (Input.GetKeyDown(KeyCode.DownArrow)) {
				lastMove = new Vector2(0, -1);
				Move(lastMove);
			}

			// Android movement

			if (Input.touchCount > 0) {
				Touch touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began) {
					if (touch.position.y > Screen.height / 3 && touch.position.y < Screen.height * 2 / 3) {
						if (touch.position.x > (Screen.width / 2)) {
							lastMove = new Vector2(1, 0);
						}
						if (touch.position.x < (Screen.width / 2)) {
							lastMove = new Vector2(-1, 0);
						}
					}
					else {
						if (touch.position.y > (Screen.height / 2)) {
							lastMove = new Vector2(0, 1);
						}
						if (touch.position.y < (Screen.height / 2)) {
							lastMove = new Vector2(0, -1);
						}
					}

					Move(lastMove);
				}
			}
		}
	}

	private void Move(Vector2 move) {
		this.gameObject.transform.Translate(move);
	}
	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.CompareTag("Pillow")) {
			Debug.Log("Onto Pillow!");
			isSleeping = true;
			anim.SetBool("isSleeping", isSleeping);
		}
		if (collision.CompareTag("Wall")) {
			Debug.Log("Wall??");
			Move(lastMove * -1);
		}
	}

}
