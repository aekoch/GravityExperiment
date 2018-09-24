using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawningPanel : MonoBehaviour {

    public Spawner masterSpawner;

    [Header("UI Elements")]
    public InputField numGroupsInput;
    public InputField groupSeperationInput;
    public InputField particlesPerGroupInput;
    public InputField groupRadiusInput;
    public InputField groupInitialVelocityInput;

    public Button respawnButton;

	// Use this for initialization
	void Start () {
        numGroupsInput.text = masterSpawner.numGroups.ToString();
        groupSeperationInput.text = masterSpawner.radius.ToString();
        particlesPerGroupInput.text = masterSpawner.groupParticles.ToString();
        groupRadiusInput.text = masterSpawner.groupRadius.ToString();
        groupInitialVelocityInput.text = masterSpawner.spawnVelocity.ToString();

        numGroupsInput.onValueChanged.AddListener(ListenNumGroupsInput);
        groupSeperationInput.onValueChanged.AddListener(ListenGroupSeperationInput);
        particlesPerGroupInput.onValueChanged.AddListener(ListenParticlesPerGroupInput);
        groupRadiusInput.onValueChanged.AddListener(ListenGroupRadiusInput);
        groupInitialVelocityInput.onValueChanged.AddListener(ListenInitialVelocityInput);
        respawnButton.onClick.AddListener(ListenRespawnButton);
    }

    private void ListenNumGroupsInput(string strInput) {
        int value = int.Parse(strInput);
        masterSpawner.numGroups = value;
    }

    private void ListenGroupSeperationInput(string strInput) {
        int value = int.Parse(strInput);
        masterSpawner.radius = value;
    }

    private void ListenParticlesPerGroupInput(string strInput) {
        int value = int.Parse(strInput);
        masterSpawner.groupParticles = value;
    }

    private void ListenGroupRadiusInput(string strInput) {
        float value = int.Parse(strInput);
        masterSpawner.groupRadius = value;
    }

    private void ListenInitialVelocityInput(string strInput) {
        float value = int.Parse(strInput);
        masterSpawner.spawnVelocity = value;
    }

    private void ListenRespawnButton() {
        masterSpawner.Respawn();
    }
}
