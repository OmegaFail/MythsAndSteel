using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [Header("TOUCHES")]
    [SerializeField] private KeyCode escapeEvent = KeyCode.Escape;
    [SerializeField] private KeyCode MoreInfoUnit = KeyCode.LeftShift;
    [SerializeField] private KeyCode MakeAction = KeyCode.KeypadEnter;
    [SerializeField] private KeyCode SkipPhase = KeyCode.Space;
    [SerializeField] private KeyCode OpenRenfort = KeyCode.Tab;

    [Header("INFOS TOUCHES")]
    [SerializeField] private float _timeToWaitForSkipPhase = 1;
    float t = 0;
    bool hasShowPanel = false;

    [SerializeField] float SkipPhaseStartWidth = 0;

    [Header("RENFORT PHASE SCRIPT")]
    [SerializeField] RenfortPhase _renfortPhase = null;

    private void Start()
    {
        UIInstance.Instance.SkipPhaseImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, UIInstance.Instance.SkipPhaseImage.GetComponent<RectTransform>().sizeDelta.y);
    }

    void Update(){
        //Pour quitter la phase d'?v?nement qui permet de choisir une case ou une unit?
        if(Input.GetKeyDown(escapeEvent) && (GameManager.Instance.ChooseUnitForEvent || GameManager.Instance.ChooseTileForEvent)){
            GameManager.Instance.CancelEvent();
        }

        //Quand on shiftclic  sur le plateau
        if(Input.GetKeyDown(MoreInfoUnit)){
            if(GameManager.Instance.ActualTurnPhase != MYthsAndSteel_Enum.PhaseDeJeu.Activation)
            {
                RaycastManager.Instance._mouseCommand.QuitShiftPanel();
                RaycastManager.Instance._mouseCommand._checkIfPlayerAsClic = true;
            }
        }
        //Quand on relache le shift+clic
        if(Input.GetKeyUp(MoreInfoUnit))
        {
            if(GameManager.Instance.ActualTurnPhase != MYthsAndSteel_Enum.PhaseDeJeu.Activation)
            {
                RaycastManager.Instance._mouseCommand.QuitShiftPanel();
                RaycastManager.Instance._mouseCommand._checkIfPlayerAsClic = false;
                RaycastManager.Instance._mouseCommand._hasCheckUnit = false;
            }
        }

        //Clic sur le plateau ou une unit?
        if(Input.GetMouseButtonDown(0) && !Input.GetKey(MoreInfoUnit)){
            if(EventSystem.current.IsPointerOverGameObject() == false)
            {
                if(GameManager.Instance.ActualTurnPhase != MYthsAndSteel_Enum.PhaseDeJeu.Activation)
                {
                    RaycastManager.Instance.Select();
                }
            }
        }

        //Poser la zone d'orgone ? sa nouvelle position
        if(Input.GetMouseButtonUp(0) && OrgoneManager.Instance.Selected){
            OrgoneManager.Instance.ReleaseZone();
        }

        //Quand le joueur clic sur entr?e pour valider une action
        if(Input.GetKeyDown(MakeAction)){
            if(Mouvement.Instance.IsInMouvement && Mouvement.Instance._selectedTileId.Count > 1){
                if(TilesManager.Instance.TileList[Mouvement.Instance._selectedTileId[Mouvement.Instance._selectedTileId.Count - 1]].GetComponent<TileScript>().Unit == null)
                {
                    Mouvement.Instance.ApplyMouvement();
                    Mouvement.Instance.DeleteChildWhenMove();
                }
            }
            else
            {
                Mouvement.Instance.StopMouvement(true);
            }
        }

        //Pour passer une phase rapidement
        if(GameManager.Instance.IsInTurn && !GameManager.Instance.ChooseTileForEvent && !GameManager.Instance.ChooseUnitForEvent)
        {
            if(Input.GetKeyDown(SkipPhase))
            {
                t = 0;
            }
            if(Input.GetKey(SkipPhase) && !hasShowPanel && !Attaque.Instance.Selected && !Mouvement.Instance.Selected && !OrgoneManager.Instance.Selected)
            {
                if(GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.Activation)
                {
                    if(GameManager.Instance.ActivationPhase.J1CarteChoisie && GameManager.Instance.ActivationPhase.J2CarteChoisie)
                    {
                        ClicToSkipPhase();
                    }
                }
                else
                {
                    ClicToSkipPhase();
                }
            }
            if(Input.GetKeyUp(SkipPhase))
            {
                UIInstance.Instance.SkipPhaseImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, UIInstance.Instance.SkipPhaseImage.GetComponent<RectTransform>().sizeDelta.y);
            }
        }

        //Quand tu cliques ur une case
        if(Input.GetMouseButtonDown(1) && RaycastManager.Instance.Tile != null)
        {
            if(!GameManager.Instance.ChooseTileForEvent && !GameManager.Instance.ChooseUnitForEvent)
            {
                if(!Mouvement.Instance.Selected && !Attaque.Instance.Selected)
                {
                    //Si l'usine de l'Arm?e Bleu est s?lectionn?e et c'est le tour du joueur de l'Arm?e Bleu.
                    if(RaycastManager.Instance.Tile.GetComponent<TileScript>().TerrainEffectList.Contains(MYthsAndSteel_Enum.TerrainType.UsineBleu) &&
                        !GameManager.Instance.IsPlayerRedTurn && (GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.ActionJ1
                        || GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.ActionJ2) && !PlayerScript.Instance.BluePlayerInfos.HasCreateUnit)
                    {
                        RaycastManager.Instance._mouseCommand.MenuRenfortUI(false);
                        _renfortPhase.CreateRenfort(false);
                    }

                    //Si l'usine de l'Arm?e Rouge est s?lectionn?e et c'est le tour du joueur de l'Arm?e Rouge.
                    if(RaycastManager.Instance.Tile.GetComponent<TileScript>().TerrainEffectList.Contains(MYthsAndSteel_Enum.TerrainType.UsineRouge)
                        && GameManager.Instance.IsPlayerRedTurn && (GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.ActionJ1
                        || GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.ActionJ2) && !PlayerScript.Instance.RedPlayerInfos.HasCreateUnit)
                    {
                        RaycastManager.Instance._mouseCommand.MenuRenfortUI(true);
                        _renfortPhase.CreateRenfort(true);
                    }
                }
                else if(Attaque.Instance.Selected)
                {
                    RaycastManager.Instance.SelectTileForAttack();
                }
            }
            else
            {
                RaycastManager.Instance.Deselect();
            }
        }

        //Ouvrir/fermer le menu renfort
        if(Input.GetKeyDown(OpenRenfort) && 
            (GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.ActionJ1 ||
            GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.ActionJ2 ||
            GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.OrgoneJ1 ||
            GameManager.Instance.ActualTurnPhase == MYthsAndSteel_Enum.PhaseDeJeu.OrgoneJ2))
        {
            RaycastManager.Instance._mouseCommand.MenuRenfortUI(GameManager.Instance.IsPlayerRedTurn);
            _renfortPhase.CreateRenfort(GameManager.Instance.IsPlayerRedTurn? true : false);
        }
        if(Input.GetKeyUp(OpenRenfort))
        {
            RaycastManager.Instance._mouseCommand.QuitRenfortPanel();
        }
    }

    /// <summary>
    /// Quand le joueur peut changer de phase
    /// </summary>
    void ClicToSkipPhase()
    {
        if((GameManager.Instance.IsPlayerRedTurn && !OrgoneManager.Instance.RedPlayerZone.GetComponent<ZoneOrgone>().IsInValidation) ||
                           (!GameManager.Instance.IsPlayerRedTurn && !OrgoneManager.Instance.BluePlayerZone.GetComponent<ZoneOrgone>().IsInValidation))
        {
            t += Time.deltaTime;
            UIInstance.Instance.SkipPhaseImage.GetComponent<RectTransform>().sizeDelta = new Vector2(SkipPhaseStartWidth * (t / _timeToWaitForSkipPhase), UIInstance.Instance.SkipPhaseImage.GetComponent<RectTransform>().sizeDelta.y);
            if(t > _timeToWaitForSkipPhase)
            {
                UIInstance.Instance.ShowValidationPanel("Passer ? la phase suivante", "?tes-vous sur de vouloir passer ? la phase suivante? En passant la phase vous n'aurez pas la possibilit? de revenir en arri?re.");
                GameManager.Instance.CliCToChangePhase();
                GameManager.Instance._eventCallCancel += CancelSkipPhase;
                GameManager.Instance._eventCall += SkipPhaseFunc;
                hasShowPanel = true;
                t = 0;
                UIInstance.Instance.SkipPhaseImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, UIInstance.Instance.SkipPhaseImage.GetComponent<RectTransform>().sizeDelta.y);
            }
        }
    }

    /// <summary>
    /// Quand le joueur accepte de changer de phase
    /// </summary>
    void SkipPhaseFunc(){
        hasShowPanel = false;
    }

    /// <summary>
    /// Quand le joueur annule le changement de phase
    /// </summary>
    void CancelSkipPhase(){
        hasShowPanel = false;
        t = 0;
        GameManager.Instance._eventCallCancel = null;
        UIInstance.Instance.ActivateNextPhaseButton();
    }
}
