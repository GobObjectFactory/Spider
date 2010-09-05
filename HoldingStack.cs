﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class HoldingStack : List<HoldingInfo>
    {
        public HoldingStack()
        {
        }

        public void Push(HoldingInfo item)
        {
            Add(item);
        }

        public int Suits
        {
            get
            {
                if (Count == 0)
                {
                    return 0;
                }
                return this[Count - 1].Suits;
            }
        }

        public HoldingSet Set
        {
            get
            {
                return new HoldingSet(this, Count);
            }
        }

        public IEnumerable<HoldingSet> Sets
        {
            get
            {
                for (int i = 0; i <= Count; i++)
                {
                    yield return new HoldingSet(this, i);
                }
            }
        }
    }
}