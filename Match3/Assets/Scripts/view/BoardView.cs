﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoardView : MonoBehaviour
{

    public PieceView[,] pieces { get; private set; }
    private GameObject pieceContainer;
    private int countClick;
    private PieceView first;
    private PieceView second;
    private List<Piece> piecesToDestroy;
    private List<Piece> pendingPiecesToDestroy;
    private List<List<Piece>> newPieces;
    private float speedSwap = 0.3f;
    void Awake()
    {
        pieceContainer = transform.Find("Pieces").gameObject;
        DrawSession();

        LogView();
    }

    private void DrawSession()
    {
        pieces = new PieceView[MatchController.ME.match.board.GetLength(0),MatchController.ME.match.board.GetLength(1)];

        float initalX = 0;
        float initalY = 800f;
        
        for (int i = 0; i < MatchController.ME.match.board.GetLength(0); i++)
        {
            for (int j = 0; j < MatchController.ME.match.board.GetLength(1); j++)
            {
                GameObject gb = (GameObject) Instantiate(Resources.Load("Prefab/Piece"), pieceContainer.transform);
                PieceView pc = gb.AddComponent<PieceView>();
                pc.Initate(MatchController.ME.match.board[i,j]);
                pc.piecePosition.anchoredPosition = new Vector2(initalX,initalY);
                pieces[i,j] = pc;
                initalX += 265f;
                pc.button.onClick.AddListener(()=>PieceChosen(pc));

            }
            initalX = 0;
            initalY -= 256;
        }

        Invoke("CheckResult",3);
    }
    
    private void PieceChosen(PieceView p)
    {
        if (countClick == 0)
        {
            first = p;
            countClick++;
            return;
        }

        second = p;
        countClick = 0;

        piecesToDestroy = MatchController.ME.ExecuteClassicMovement(first.currentPiece, second.currentPiece);
        SwapPieces();
        
    }
    
    private void CheckResult()
    {
       
        if (piecesToDestroy != null && piecesToDestroy.Count > 0)
        {
            ConfirmSwap(first, second);
            OnFinishSwap();
            LogView();
            DestroyPieces(piecesToDestroy);
            GetNewPieces();
            RePosition();
            Invoke("DropNewPieces", 0.5f);
            piecesToDestroy.Clear();

        }
        else if (first != null && second != null)
        {
            SwapPieces(false);
        }
        else
        {
            pendingPiecesToDestroy = MatchController.ME.PendingPieces();
            if (pendingPiecesToDestroy != null && pendingPiecesToDestroy.Count > 0)
            {
                DestroyPieces(pendingPiecesToDestroy);
                GetNewPieces();
                RePosition();
                Invoke("DropNewPieces", 0.5f);
            }
            else
            {
                Debug.Log("NO MATCHES");
            
            }
        }
    }
    
    private void SwapPieces(bool withCallback = true)
    {
        for (var i = 0; i < pieces.GetLength(0); i++)
        {
            for (int j = 0; j < pieces.GetLength(1); j++)
            {
                pieces[i, j].piecePhysics.bodyType = RigidbodyType2D.Static;
            }
        }


        Vector2 saveFirst = first.piecePosition.anchoredPosition;
        Vector2 saveSecond = second.piecePosition.anchoredPosition;
         
        first.piecePosition.DOAnchorPos(saveSecond, speedSwap);
        second.piecePosition.DOAnchorPos(saveFirst, speedSwap).OnComplete(()=>
        {
            if (withCallback) CheckResult();
            else OnFinishSwap();
        });
    }

    private void ConfirmSwap(PieceView ft, PieceView sc)
    {
        pieces[sc.currentPiece.tupplePosition.line, sc.currentPiece.tupplePosition.column] = second;
        pieces[ft.currentPiece.tupplePosition.line, ft.currentPiece.tupplePosition.column] = first;
    }

    private void OnFinishSwap()
    {
        for (var i = 0; i < pieces.GetLength(0); i++)
        {
            for (int j = 0; j < pieces.GetLength(1); j++)
            {
                pieces[i,j].piecePhysics.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }

    private void DestroyPieces(List<Piece> piecesToDestroy)
    {
        foreach (Piece pc in piecesToDestroy)
        {
            Tupple pos = new Tupple(pc.tupplePosition.line, pc.tupplePosition.column);
            PieceView current = pieces[pos.line, pos.column];
            if (current != null)
            {
                //Debug.Log("DESTROY :" + current.currentPiece.type + " | " + current.currentPiece.tupplePosition);
                current.DestroyPiece();
                //current.SetOld();
                pieces[pos.line, pos.column] = null;
 
            }
        }

    }

    private void RePosition()
    {
        //Order pieces
        for (int i = pieces.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = pieces.GetLength(1) - 1; j >= 0; j--)
            {
                if (pieces[i, j] == null)
                {
                    int index = i - 1;

                    while (index >= 0 && pieces[i, j] == null)
                    {
                        if (pieces[index, j] != null)
                        {
                            pieces[i, j] = pieces[index, j];
                            pieces[index, j] = null;

                        }

                        index--;
                    }

                }

            }
        }

    }

    private void GetNewPieces()
    {
        newPieces = MatchController.ME.NewPieces();
        //MatchController.ME.LogGame(MatchController.ME.match.board,5);
       
    }

    private void DropNewPieces()
    {
        float initalX = 0;
        float initalY = 800f;

        List<PieceView> newPiecesView = new List<PieceView>();

        for (int i = 0; i < newPieces.Count; i++)
        {
            for (int j = 0; j < newPieces[i].Count; j++)
            {
                GameObject gb = (GameObject)Instantiate(Resources.Load("Prefab/Piece"), pieceContainer.transform);
                PieceView pc = gb.AddComponent<PieceView>();
                pc.Initate(newPieces[i][j]);
                pc.piecePosition.anchoredPosition = new Vector2(initalX, initalY);
                initalY -= 256f;
                pc.button.onClick.AddListener(() => PieceChosen(pc));
                newPiecesView.Add(pc);

                pieces[pc.currentPiece.tupplePosition.line, pc.currentPiece.tupplePosition.column] = pc;

            }
            initalY = 800;
            initalX += 265;
        }

     
        //RETUPLE
        for (int i = pieces.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = pieces.GetLength(1) - 1; j >= 0; j--)
            {
                //if (pieces[i, j] != null)
                //{
                    pieces[i, j].currentPiece.tupplePosition = new Tupple(i, j);
                    pieces[i, j].UpdateText();
                //}
            }
        }

        first = second = null;
        Invoke("CheckResult", 2);

    }

    public void LogView()
    {
        
        string str = "Log View" + '\n';
        int countBreak = 0;
        int breakLine = 5;

        for (int i = 0; i < pieces.GetLength(0); i++)
        {
            for (int j = 0; j < pieces.GetLength(1); j++)
            {
                countBreak++;

                if (countBreak == breakLine)
                {
                    if (pieces[i,j] != null) str += pieces[i,j].currentPiece.type;
                    else str += "X";

                    str += '\n';
                    countBreak = 0;
                }
                else
                {
                    if (pieces[i,j] != null) str += pieces[i,j].currentPiece.type + ",";
                    else str += "X" + ",";
                }

            }
           
        }

        Debug.Log(str);
    }

}
