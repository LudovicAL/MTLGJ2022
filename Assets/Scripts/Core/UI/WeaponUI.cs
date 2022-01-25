using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private Button template;
	[SerializeField] private AudioClip switchAudio;

    public Action<int> activeWeaponChanged;
    private Dictionary<int, Button> buttons = new Dictionary<int, Button>();

    public void ChangeActiveWeapon(int i)
    {
        buttons.TryGetValue(i, out Button targetBtn);

        if (targetBtn != null)
        {
            foreach (KeyValuePair<int, Button> btn in buttons)
            {
                Color color = (btn.Key == i) ? selectedColor : inactiveColor;
                btn.Value.image.color = color;
                btn.Value.GetComponentInChildren<TMP_Text>().color = color;
            }
            activeWeaponChanged?.Invoke(i);
			if (AudioManager.Instance) {
				AudioManager.Instance.PlayClip(switchAudio);
			}
        }
    }

    public void AddWeaponToUI(int i)
    {
        Button newButton = Instantiate(template, this.transform);
        TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = $"{i + 1}";
        buttonText.color = inactiveColor;
        newButton.image.color = inactiveColor;
        newButton.onClick.AddListener(() => ChangeActiveWeapon(i));
        newButton.gameObject.SetActive(true);

        buttons.Add(i, newButton);
    }
}
