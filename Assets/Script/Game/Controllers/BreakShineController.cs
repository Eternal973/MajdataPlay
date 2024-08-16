using MajdataPlay.Interfaces;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Controllers
{
    public class BreakShineController : MonoBehaviour
    {
        public IFlasher? parent = null;

        SpriteRenderer spriteRenderer;
        GamePlayManager gpManager => GamePlayManager.Instance;
        // Start is called before the first frame update
        void Start()
        {

        }
        // Update is called once per frame
        void Update()
        {
            if (parent is not null && parent.CanShine())
            {
                var extra = Math.Max(Mathf.Sin(gpManager.GetFrame() * 0.17f) * 0.5f, 0);
                spriteRenderer.material.SetFloat("_Brightness", 0.95f + extra);
            }
        }
        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}