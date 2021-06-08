﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miku.Lua
{
	enum OpCode
	{
		ISLT,ISGE,ISLE,ISGT,
		ISEQV,ISNEV,ISEQS,ISNES,ISEQN,ISNEN,ISEQP,ISNEP,
		ISTC,ISFC,IST,ISF,ISTYPE,ISNUM,
		MOV,NOT,UNM,LEN,
		ADDVN,SUBVN,MULVN,DIVVN,MODVN,
		ADDNV,SUBNV,MULNV,DIVNV,MODNV,
		ADDVV,SUBVV,MULVV,DIVVV,MODVV,
		POW,CAT,
		KSTR,KCDATA,KSHORT,KNUM,KPRI,KNIL, // DONE
		UGET,USETV,USETS,USETN,USETP,UCLO,FNEW,
		TNEW,TDUP,GGET,GSET,TGETV,TGETS,TGETB,TGETR,TSETV,TSETS,TSETB,TSETM,TSETR,
		CALLM,CALL,CALLMT,CALLT,ITERC,ITERN,VARG,ISNEXT,
		RETM,RET,RET0,RET1,
		FORI,JFORI,
		FORL,IFORL,JFORL,
		ITERL,IITERL,JITERL,
		LOOP,ILOOP,JLOOP,
		JMP,
		FUNCF,IFUNCF,JFUNCF,FUNCV,IFUNCV,JFUNCV,FUNCC,FUNCCW
	}
}
