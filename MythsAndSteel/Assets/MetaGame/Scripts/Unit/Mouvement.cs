using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouvement : MonoSingleton<Mouvement>
{
    #region Variables
    [Header("LISTES DES CASES")]
    [SerializeField] private int[] neighbourValue; // +1 +9 +10...

    [SerializeField] private List<int> newNeighbourId = new List<int>(); // Voisins atteignables avec le range de l'unité.
    public List<int> _selectedTileId => selectedTileId;

    [SerializeField] private List<int> selectedTileId = new List<int>(); // Cases selectionnées par le joueur.
    public List<int> _newNeighbourId => newNeighbourId;

    [SerializeField] private float speed = 1; // Speed de déplacement de l'unité 

    private GameObject mStart; // mT Start. 
    private GameObject mEnd; // mT End.
    private GameObject mUnit; // mT Unité.

    private List<int> temp = new List<int>(); //

    //Déplacement restant de l'unité au départ
    int MoveLeftBase = 0;

    [Header("INFOS DE L UNITE")]
    //Est ce que l'unité a commencé à choisir son déplacement
    [SerializeField] private bool _isInMouvement;
    public bool IsInMouvement
    {
        get
        {
            return _isInMouvement;
        }
        set
        {
            _isInMouvement = value;
        }
    }

    //Est ce qu'une unité est sélectionnée
    [SerializeField] private bool _selected;
    public bool Selected
    {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;
        }
    }

    // Mouvement en cours de traitement ?
    [SerializeField] private bool _mvmtRunning = false;
    public bool MvmtRunning => _mvmtRunning;

    [Header("SPRITES POUR LES CASES")]
    [SerializeField] private Sprite _tileSprite = null;
    [SerializeField] private Sprite _emptySprite = null;
    [SerializeField] private Sprite _selectedSprite = null;
    [SerializeField] private List<MYthsAndSteel_Enum.TerrainType> EffectToCheck;

    #endregion Variables

    private void Update()
    {
        // Permet d'effectuer le moveTowards de l'unité à sa prochaine case.
        UpdatingMove(mUnit, mStart, mEnd);
    }

    /// <summary>
    /// Cette fonction "highlight" les cases atteignables par l'unité sur la case sélectionnée.
    /// </summary>
    /// <param name="tileId">Tile centrale</param>
    /// <param name="Range">Range de l'unité</param>
    public void Highlight(int tileId, int Range)
    {
        if (Range > 0)
        {
            foreach (int ID in PlayerStatic.GetNeighbourDiag(tileId, TilesManager.Instance.TileList[tileId].GetComponent<TileScript>().Line, false))
            {
                TileScript TileSc = TilesManager.Instance.TileList[ID].GetComponent<TileScript>();
                bool i = false;
                foreach (MYthsAndSteel_Enum.TerrainType Type in TileSc.TerrainEffectList)
                {
                    Debug.Log("onlyone" + ID);

                    if (EffectToCheck.Contains(Type))
                    {
                        if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Ravin, ID) || PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Eau, ID))
                        {
                            i = true;
                            break;
                        }
                        if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Rivière_Est, tileId) && PlayerStatic.CheckDirection(tileId, ID) == "Est")
                        {
                            i = true;
                            break;
                        }
                        if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Rivière_Nord, tileId) && PlayerStatic.CheckDirection(tileId, ID) == "Nord")
                        {
                            Debug.Log("Nord");
                            i = true;
                            break;
                        }
                        if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Rivière_Sud, tileId) && PlayerStatic.CheckDirection(tileId, ID) == "Sud")
                        {
                            i = true;
                            break;
                        }
                        if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Rivière_Ouest, tileId) && PlayerStatic.CheckDirection(tileId, ID) == "Ouest")
                        {
                            i = true;
                            break;
                        }
                        if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Mont, ID) || PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Forêt, ID))
                        {
                            if (Range >= 2)
                            {
                                i = true;
                                TilesManager.Instance.TileList[ID].GetComponent<SpriteRenderer>().sprite = _selectedSprite;
                                if (!newNeighbourId.Contains(ID))
                                {
                                    newNeighbourId.Add(ID);
                                }
                                Debug.Log("Foret: " + ID + "R: " + Range);
                                Highlight(ID, Range - 2);
                                break;
                            }
                            else
                            {
                                i = true;
                                break;
                            }
                        }
                    }
                }
                if (!i)
                {
                    TilesManager.Instance.TileList[ID].GetComponent<SpriteRenderer>().sprite = _selectedSprite;
                    if (!newNeighbourId.Contains(ID))
                    {
                        newNeighbourId.Add(ID);
                    }
                    Debug.Log("Classic: " + ID + "R: " + Range);
                    Highlight(ID, Range - 1);
                }
            }
        }
    }



    /// <summary>
    /// Lance le mouvement d'une unité avec une range défini.
    /// </summary>
    /// <param name="tileId">Tile de l'unité</param>
    /// <param name="Range">Mvmt de l'unité</param>
    public void StartMvmtForSelectedUnit()
    {
        GameObject tileSelected = RaycastManager.Instance.ActualTileSelected;

        if (tileSelected != null)
        {
            mUnit = tileSelected.GetComponent<TileScript>().Unit;
            if (!mUnit.GetComponent<UnitScript>().IsMoveDone)
            {
                MoveLeftBase = mUnit.GetComponent<UnitScript>().MoveLeft;
                StartMouvement(TilesManager.Instance.TileList.IndexOf(tileSelected), mUnit.GetComponent<UnitScript>().MoveSpeed - (mUnit.GetComponent<UnitScript>().MoveSpeed - MoveLeftBase));
            }
            else
            {
                _selected = false;
            }
        }
        else
        {
            _selected = false;
        }
    }

    public void StartMouvement(int tileId, int Range)
    {
        if (!_mvmtRunning && !_isInMouvement)
        {
            _isInMouvement = true;
            selectedTileId.Add(tileId);
            List<int> ID = new List<int>();
            ID.Add(tileId);

            // Lance l'highlight des cases dans la range.
            Highlight(tileId, Range);
        }
    }

    /// <summary>
    /// Arête le Mouvement pour l'unité selectionnée (menu, cases highlights...)
    /// </summary>
    public void StopMouvement(bool forceStop)
    {
        foreach (int Neighbour in newNeighbourId) // Supprime toutes les tiles.
        {
            TilesManager.Instance.TileList[Neighbour].GetComponent<SpriteRenderer>().sprite = _emptySprite; // Assigne un sprite empty à toutes les anciennes cases "neighbour".
        }
        if (RaycastManager.Instance.ActualTileSelected != null) // Si une case était séléctionnée.
        {
            //Tiles.Instance._actualTileSelected.GetComponent<TileScript>().Unit.GetComponent<UnitScript>().DemandMenu.enabled = false;
            //Tiles.Instance._actualTileSelected.GetComponent<TileScript>().Unit.GetComponent<UnitScript>().Menu.enabled = false;
        }
        foreach (int NeighbourSelect in selectedTileId) // Si un path de mvmt était séléctionné.
        {
            TilesManager.Instance.TileList[NeighbourSelect].GetComponent<SpriteRenderer>().sprite = _emptySprite;
            TilesManager.Instance.TileList[NeighbourSelect].GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);
        }
        // Clear de toutes les listes et stats.
        selectedTileId.Clear();
        newNeighbourId.Clear();
        mStart = null;
        mEnd = null;
        _isInMouvement = false;
        _selected = false;

        mUnit.GetComponent<UnitScript>().MoveLeft = forceStop? MoveLeftBase : mUnit.GetComponent<UnitScript>().MoveLeft;
        mUnit.GetComponent<UnitScript>().checkMovementLeft();

        mUnit = null;

        RaycastManager.Instance.ActualTileSelected = null;

        _mvmtRunning = false;
    }

    /// <summary>
    /// Ajoute la tile à TileSelected. Pour le mvmt du joueur => Check egalement toutes les conditions de déplacement.
    /// </summary>
    /// <param name="tileId">Tile</param>
    public void AddMouvement(int tileId)
    {
        bool check = false;
        if (_isInMouvement)
        {
            if (newNeighbourId.Contains(tileId)) // Si cette case est dans la range de l'unité.
            {
                if (selectedTileId.Contains(tileId)) // Si cette case est déjà selectionnée.
                {
                    // Supprime toutes les cases sélectionnées à partir de l'ID tileId.
                    for(int i = selectedTileId.IndexOf(tileId); i < selectedTileId.Count; i++){
                        if(PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Forêt, selectedTileId[i]) || PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Mont, selectedTileId[i]))
                        {
                            Debug.Log("REMOVE");
                            mUnit.GetComponent<UnitScript>().MoveLeft += 2; // Redistribution du Range à chaque suppression de case.
                            temp.Add(selectedTileId[i]);
                            TilesManager.Instance.TileList[selectedTileId[i]].GetComponent<SpriteRenderer>().sprite = _selectedSprite; // Repasse les sprites en apparence "séléctionnable".
                        }
                        else
                        {
                            Debug.Log("REMOVE");
                            mUnit.GetComponent<UnitScript>().MoveLeft++; // Redistribution du Range à chaque suppression de case.
                            temp.Add(selectedTileId[i]);
                            TilesManager.Instance.TileList[selectedTileId[i]].GetComponent<SpriteRenderer>().sprite = _selectedSprite; // Repasse les sprites en apparence "séléctionnable".
                        }
                    }
                    foreach (int i in temp)
                    {
                        selectedTileId.Remove(i);
                    }
                    temp.Clear();

                }
                // Sinon, si cette case est bien voisine de l'ancienne selection. 
                else if (PlayerStatic.IsNeighbour(tileId, selectedTileId[selectedTileId.Count - 1], TilesManager.Instance.TileList[tileId].GetComponent<TileScript>().Line, false))
                {
                    // et qu'il reste du mvmt, on assigne la nouvelle case selectionnée à la liste SelectedTile.
                    if (mUnit.GetComponent<UnitScript>().MoveLeft > 0)
                    {
                        if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Forêt, tileId) || PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Mont, tileId))
                        {
                            if (mUnit.GetComponent<UnitScript>().MoveLeft >= 2)
                            {
                                check = true;
                                mUnit.GetComponent<UnitScript>().MoveLeft -= 2; // sup 2 mvmt.
                                selectedTileId.Add(tileId);
                                TilesManager.Instance.TileList[tileId].GetComponent<SpriteRenderer>().sprite = _tileSprite;
                            }
                            else
                            {
                                check = true;
                                Debug.Log("La tile d'ID : " + tileId + " est une foret ou un mont.");
                            }
                        }
                        if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Rivière_Est, tileId) && PlayerStatic.CheckDirection(tileId, selectedTileId[selectedTileId.Count - 1]) == "Est")
                        {
                            Debug.Log("La tile d'ID : " + tileId + " est séparée par une rivière de la tile d'ID :" + selectedTileId[selectedTileId.Count - 1]);
                        }
                        else if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Rivière_Nord, tileId) && PlayerStatic.CheckDirection(tileId, selectedTileId[selectedTileId.Count - 1]) == "Nord")
                        {
                            Debug.Log("La tile d'ID : " + tileId + " est séparée par une rivière de la tile d'ID :" + selectedTileId[selectedTileId.Count - 1]);
                        }
                        else if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Rivière_Sud, tileId) && PlayerStatic.CheckDirection(tileId, selectedTileId[selectedTileId.Count - 1]) == "Sud")
                        {
                            Debug.Log("La tile d'ID : " + tileId + " est séparée par une rivière de la tile d'ID :" + selectedTileId[selectedTileId.Count - 1]);
                        }
                        else if (PlayerStatic.CheckTiles(MYthsAndSteel_Enum.TerrainType.Rivière_Ouest, tileId) && PlayerStatic.CheckDirection(tileId, selectedTileId[selectedTileId.Count - 1]) == "Ouest")
                        {
                            Debug.Log("La tile d'ID : " + tileId + " est séparée par une rivière de la tile d'ID :" + selectedTileId[selectedTileId.Count - 1]);
                        }
                        else if(!check)
                        {
                            mUnit.GetComponent<UnitScript>().MoveLeft--; // sup 1 mvmt.
                            selectedTileId.Add(tileId);
                            TilesManager.Instance.TileList[tileId].GetComponent<SpriteRenderer>().sprite = _tileSprite;
                        }
                    }
                }
                // Sinon cette case est trop loin de l'ancienne seletion.
                else
                {
                    Debug.Log("La tile d'ID : " + tileId + " est trop loin de la tile d'ID: " + selectedTileId[selectedTileId.Count - 1]);
                }
            }
            // Sinon cette case est hors de la range de l'unité.
            else
            {
                Debug.Log("La tile d'ID : " + tileId + " est trop loin de la tile d'ID: " + selectedTileId[selectedTileId.Count - 1]);
            }
        }
    }


    int MvmtIndex = 1; // Numéro du mvmt actuel dans la liste selectedTileId;
    [SerializeField] bool Launch = false; // Evite les répétitions dans updatingmove();

    /// <summary>
    /// Assigne le prochain mouvement demandé à l'unité. Change les stats de l'ancienne et de la nouvelle case. Actualise les informations de position de l'unité.
    /// </summary>
    public void ApplyMouvement()
    {
        GameObject tileSelected = RaycastManager.Instance.ActualTileSelected;

        if (tileSelected != null && (_selectedTileId.Count != 0 && _selectedTileId.Count != 1))
        {
            _mvmtRunning = true;
            mStart = tileSelected; // Assignation du nouveau départ.
            mEnd = TilesManager.Instance.TileList[selectedTileId[MvmtIndex]];  // Assignation du nouvel arrirée.

            foreach (int Neighbour in newNeighbourId) // Désactive toutes les cases selectionnées par la fonction Highlight.
            {
                if (!selectedTileId.Contains(Neighbour))
                {
                    TilesManager.Instance.TileList[Neighbour].GetComponent<SpriteRenderer>().sprite = _emptySprite; // Assigne un sprite empty à toutes les anciennes cases "neighbour"
                }
            }
            Debug.Log("Actual tile target: " + TilesManager.Instance.TileList[selectedTileId[MvmtIndex]]);
        }
    }

    /// <summary>
    /// Coroutine d'attente entre chaque case. Probablement pendant ce temps que l'on devra appliquer les effets de case.
    /// </summary>
    /// <returns>Temps à définir</returns>
    IEnumerator MvmtEnd()
    {
        mEnd.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("empty"); // La case dépassée redevient une "empty"
        mEnd.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255); // La case reprend sa couleur d'origine.
        mEnd.GetComponent<TileScript>().AddUnitToTile(mStart.GetComponent<TileScript>().Unit); // L'unité de la case d'arrivée devient celle de la case de départ.
        mStart.GetComponent<TileScript>().RemoveUnitFromTile(); // L'ancienne case n'a plus d'unité.
        mUnit = mEnd.GetComponent<TileScript>().Unit;
        mUnit.GetComponent<UnitScript>().ActualTiledId = TilesManager.Instance.TileList.IndexOf(mEnd);
        RaycastManager.Instance.ActualTileSelected = mEnd;
        mStart = mEnd;
        mEnd = null;

        yield return new WaitForSeconds(1); // Temps d'attente.
        if (MvmtIndex < selectedTileId.Count - 1) // Si il reste des mvmts à effectuer dans la liste SelectedTile.
        {
            MvmtIndex++;
            ApplyMouvement();
        }
        else // Si il ne reste aucun mvmt dans la liste SelectedTile.
        {
            MvmtIndex = 1;
            StopMouvement(false); // Arête le mvmt de l'unité.
        }
        Launch = false; // Reset de la bool Launch
    }

    float speed1;

    /// <summary>
    /// Cette fonction lance l'animation de translation de l'unité entre les cases.
    /// </summary>
    /// <param name="Unit">The unit gameobject.</param>
    /// <param name="StartPos">start position tile</param>
    /// <param name="EndPos">end position tile</param>
    void UpdatingMove(GameObject Unit, GameObject StartPos, GameObject EndPos)
    {
        if (Unit != null && StartPos != null && EndPos != null)
        {
            Unit.transform.position = Vector2.MoveTowards(Unit.transform.position, EndPos.transform.position, speed1); // Application du mvmt.
            speed1 = Mathf.Abs((Vector2.Distance(mUnit.transform.position, mEnd.transform.position) * speed * Time.deltaTime)); // Régulation de la vitesse. (effet de ralentissement) 
            if (Vector2.Distance(mUnit.transform.position, mEnd.transform.position) <= 0.05f && Launch == false) // Si l'unité est arrivée.
            {
                Launch = true;
                StartCoroutine(MvmtEnd()); // Lancer le prochain mvmt avec délai. 
            }
            else // Sinon appliqué l'opacité à la case d'arrivée en fonction de la distance unité - arrivée.
            {
                mEnd.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Vector2.Distance(mUnit.transform.position, mEnd.transform.position));
            }
        }
    }
}


