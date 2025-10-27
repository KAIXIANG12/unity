using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface GridItem
    {
        void InitGridItem(int id);
        RectTransform GetRectTransform();
    }
}