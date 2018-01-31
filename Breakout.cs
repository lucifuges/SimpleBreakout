using System.Collections;
using System.Linq;
using UnityEngine;

public class Breakout : MonoBehaviour
{
	Sprite sprite;
	[RuntimeInitializeOnLoadMethod]
	static void Init()
	{
		new GameObject("Breakout")
			.AddComponent<Breakout>()
				.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
	}
	SpriteRenderer CreateSpriteObject(string name, Color color, Vector3 position, Vector2 size)
	{
		var result = new GameObject(name).AddComponent<SpriteRenderer>();
		result.sprite = sprite;
		result.color = color;
		result.transform.position = position;
		result.transform.localScale = new Vector3(size.x, size.y, 1);
		return result;
	}
	const float barSpeed = 3;
	const float ballSpeed = 3;
	const float ballAccel = 1.02f;
	IEnumerator Start()
	{
		// Setup camera
		Camera.main.orthographic = true;
		Camera.main.transform.position = Vector3.zero;
		// Create audio clip for hit SE.
		var se = gameObject.AddComponent<AudioSource>();
		(se.clip = AudioClip.Create("hit", 600, 1, 44100, false))
			.SetData(Enumerable.Range(0, 600).Select(t => Mathf.Sin(t * 0.1f)).ToArray(), 0);
		// Create sprites
		var field = CreateSpriteObject("Field", Color.black, new Vector3(0, 0, 2), new Vector2(120, 200));
		var blocks = Enumerable.Range(0, 40).Select(i =>
			CreateSpriteObject("Block " + i, Color.red, new Vector3(-1.7f + ((3.4f / 4) * (i % 5)), 3 - (0.2f * (i / 5)), 1), new Vector2(20, 4))
		).ToList();
		var bar = CreateSpriteObject("Bar", Color.cyan, new Vector3(0, -3, 1), new Vector2(20, 4));
		var ball = CreateSpriteObject("Ball", Color.white, new Vector3(0, -2.5f, 1), new Vector2(3, 3));
		ball.transform.rotation = Quaternion.Euler(0, 0, 45);
		// Start main loop
		var velocity = new Vector3(1, 1, 0).normalized * ballSpeed;
		for (;;)
		{
			// Move bar by user input
			var barpos = bar.transform.position;
			barpos.x = Mathf.Clamp(barpos.x + Input.GetAxisRaw("Horizontal") * Time.deltaTime * barSpeed, -2, 2);
			bar.transform.position = barpos;
			// Move ball by velocity
			ball.transform.Translate(velocity * Time.deltaTime, Space.World);
			// Check collision with blocks and remove it if collided
			var ballpos = ball.transform.position;
			if (blocks.RemoveAll(block => {
				if (block.bounds.Intersects(ball.bounds))
				{
					// Bounce by angle from center of collided block
					if (Mathf.Abs(Vector3.Dot(Vector3.up, (ballpos - block.transform.position).normalized)) < 0.2f)
						velocity.x *= -1;
					else
						velocity.y *= -1;
					velocity *= ballAccel; // Speed up!
					Destroy(block.gameObject);
					se.Play();
					return true;
				}
				return false;
			}) <= 0) // Detect no blocks broke
			{
				// Check fumble
				if (ballpos.y < -4)
				{
					field.color = Color.blue;
					yield break;
				}
				// Check vertical bounce
				if ((velocity.y < 0 && bar.bounds.Intersects(ball.bounds)) || (velocity.y > 0 && ballpos.y > 3.9f))
				{
					velocity.y *= -1;
					se.Play();
				}
				// Check horizontal bounce
				if ((velocity.x < 0 && ballpos.x < -2.33f) || (velocity.x > 0 && ballpos.x > 2.33f))
				{
					velocity.x *= -1;
					se.Play();
				}
			}
			// Win if no blocks exist any more
			else if (blocks.Count <= 0)
			{
				field.color = Color.yellow;
				yield break;
			}
			yield return null;
		}
	}
}
