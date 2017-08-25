using System.Collections;
using System.Linq;
using UnityEngine;

public class Breakout : MonoBehaviour
{
	delegate SpriteRenderer func(string name, Color color, Vector3 position, Vector2 size);
	IEnumerator Start()
	{
		Camera.main.orthographic = true;
		Camera.main.transform.position = Vector3.zero;

		var se = gameObject.AddComponent<AudioSource>();
		(se.clip = AudioClip.Create("source", 600, 1, 44100, false))
			.SetData(Enumerable.Range(0, 600).Select(t => Mathf.Sin(t * 0.1f)).ToArray(), 0);

		var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
		func CreateSprite = (string name, Color color, Vector3 position, Vector2 size) => {
			var result = new GameObject(name).AddComponent<SpriteRenderer>();
			result.sprite = sprite;
			result.color = color;
			result.transform.position = position;
			result.transform.localScale = size;
			return result;
		};

		var field = CreateSprite("field", Color.black, new Vector3(0, 0, 2), new Vector3(120, 200, 1));
		var blocks = Enumerable.Range(0, 40).Select(i =>
			CreateSprite("block" + i, Color.red, new Vector3(-1.7f + ((3.4f / 4) * (i % 5)), 3 - (0.2f * (i / 5)), 1), new Vector2(20, 4))
		).ToList();
		var bar = CreateSprite("bar", Color.cyan, new Vector3(0, -3, 1), new Vector3(20, 4, 1));
		var ball = CreateSprite("ball", Color.white, new Vector3(0, -2.5f, 1), new Vector3(3, 3, 1));
		ball.transform.rotation = Quaternion.Euler(0, 0, 45);

		var velocity = new Vector3(2, 2, 0);
		for (;;)
		{
			var barpos = bar.transform.position;
			barpos.x = Mathf.Clamp(barpos.x + Input.GetAxisRaw("Horizontal") * Time.deltaTime * 3, -2, 2);
			bar.transform.position = barpos;

			ball.transform.Translate(velocity * Time.deltaTime, Space.World);
			var ballpos = ball.transform.position;
			if (blocks.RemoveAll(block => {
				if (block.bounds.Intersects(ball.bounds))
				{
					if (Mathf.Abs(Vector3.Dot(Vector3.up, (ballpos - block.transform.position).normalized)) < 0.1f)
						velocity.x *= -1;
					else
						velocity.y *= -1;
					Destroy(block.gameObject);
					se.Play();
					return true;
				}
				return false;
			}) <= 0)
			{
				if (ballpos.y < -4)
				{
					field.color = Color.blue;
					yield break;
				}
				if ((velocity.y < 0 && bar.bounds.Intersects(ball.bounds)) || (velocity.y > 0 && ballpos.y > 3.9f))
				{
					velocity.y *= -1;
					se.Play();
				}
				if ((velocity.x < 0 && ballpos.x < -2.33f) || (velocity.x > 0 && ballpos.x > 2.33f))
				{
					velocity.x *= -1;
					se.Play();
				}
			}
			else if (blocks.Count <= 0)
			{
				field.color = Color.yellow;
				yield break;
			}
			yield return null;
		}
	}
}
