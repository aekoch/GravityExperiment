using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopMenu : MonoBehaviour {

    public Button physicsButton;
    public GameObject physicsPanel;
    public Button spawningButton;
    public GameObject spawningPanel;
    public Button appearanceButton;
    public GameObject appearancePanel;
    public Button octreeButton;
    public GameObject octreePanel;

    private bool showPhysicsPanel = false;
    private bool showSpawningPanel = false;
    private bool showAppearancePanel = false;
    private bool showOctreePanel = false;

	// Use this for initialization
	void Start () {
        physicsButton.onClick.AddListener(ListenPhysicsButton);
        spawningButton.onClick.AddListener(ListenSpawningButton);
        appearanceButton.onClick.AddListener(ListenAppearanceButton);
        octreeButton.onClick.AddListener(ListenOctreeButton);
        physicsPanel.SetActive(showPhysicsPanel);
        spawningPanel.SetActive(showSpawningPanel);
        appearancePanel.SetActive(showAppearancePanel);
        octreePanel.SetActive(showOctreePanel);
    }

    private void ListenPhysicsButton() {
        showPhysicsPanel = !showPhysicsPanel;
        physicsPanel.SetActive(showPhysicsPanel);
    }

    private void ListenSpawningButton() {
        showSpawningPanel = !showSpawningPanel;
        spawningPanel.SetActive(showSpawningPanel);
    } 

    private void ListenAppearanceButton() {
        showAppearancePanel = !showAppearancePanel;
        appearancePanel.SetActive(showAppearancePanel);
    }

    private void ListenOctreeButton() {
        showOctreePanel = !showOctreePanel;
        octreePanel.SetActive(showOctreePanel);
    }
}
