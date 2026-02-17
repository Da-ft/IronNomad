using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private float _speedX = 0f;
    [SerializeField] private float _speedY = 1f;

    //private void Update()
    //{
    //    float offsetX = Time.time * _speedX;
    //    float offsetY = Time.time * _speedY;
    //    _renderer.material.mainTextureOffset = new Vector2(offsetX, offsetY);
    //}
}