﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneCollumnPw : Powerup {

    public override List<Piece> ExecutePowerup(Piece p)
    {
        List<Piece> lp = new List<Piece>();
        Tupple tref = p.tupplePosition;


        for (int j = 0; j < MatchController.ME.session.board.GetLength(0); j++)
        {
            lp.Add(MatchController.ME.session.board[j, tref.column]);
        }

        return lp;
    }
}
