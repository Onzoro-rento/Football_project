using UnityEngine;
using UnityEngine.UI;

namespace LowLatencyMultichannelAudio.DEMO
{
    public class SliderTextBinder : MonoBehaviour
    {
        private Text _Text;

        private void Awake()
        {
            _Text = gameObject.GetComponent<Text>();
        }

        public void OnSliderValuedChanged(float value)
        {
            _Text.text = value.ToString("F2");
        }
    }
}
