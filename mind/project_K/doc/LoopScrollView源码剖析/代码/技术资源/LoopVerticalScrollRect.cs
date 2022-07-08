using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Practice
{
    [AddComponentMenu("UI/Loop Vertical Scroll Rect", 51)]
    public class LoopVerticalScrollRect : LoopScrollRect
    {
        protected override float getSize(RectTransform item)
        {
            float size = contentSpacing;
            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.y;
            }
            else
            {
                size += LayoutUtility.GetPreferredHeight(item);
            }
            return size;
        }

        protected override float getDimension(Vector2 vector2)
        {
            return vector2.y;
        }

        protected override Vector2 getVector(float value)
        {
            return new Vector2(0, value);
        }

        protected override void Awake()
        {
            direction = LoopScrollRectDirection.Vertical;
            base.Awake();

            GridLayoutGroup layout = content.GetComponent<GridLayoutGroup>();
            if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
            {
                Debug.Log("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
            }
        }

        /// <summary>
        /// 如果向上滑，就在尾部删去一个item
        /// </summary>
        /// <param name="viewBounds"></param>
        /// <param name="contentBounds"></param>
        /// <returns></returns>
        protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            bool change = false;

            // special case: handling move several page in one frame
            if (viewBounds.max.y < contentBounds.min.y && itemTypeEnd > itemTypeStart)
            {
                int maxItemTypeStart = -1;
                if (totalCount >= 0)
                {
                    maxItemTypeStart = Mathf.Max(0, totalCount - (itemTypeEnd - itemTypeStart));
                }
                float currentSize = contentBounds.size.y;//所有内容总高
                float elementSize = (currentSize - contentSpacing * (currentLines - 1)) / currentLines;
                returnToTempPool(true, itemTypeEnd - itemTypeStart);
                itemTypeStart = itemTypeEnd;//一旦回收所有，就重置

                int offsetCount = Mathf.FloorToInt((contentBounds.min.y - viewBounds.max.y) / (elementSize + contentSpacing));
                if (maxItemTypeStart >= 0 && itemTypeStart + offsetCount * contentConstraintCount > maxItemTypeStart)
                {
                    offsetCount = Mathf.FloorToInt((float)(maxItemTypeStart - itemTypeStart) / contentConstraintCount);
                }
                itemTypeStart += offsetCount * contentConstraintCount;
                if (totalCount >= 0)
                {
                    itemTypeStart = Mathf.Max(itemTypeStart, 0);
                }
                itemTypeEnd = itemTypeStart;

                float offset = offsetCount * (elementSize + contentSpacing);
                content.anchoredPosition -= new Vector2(0, offset + (reverseDirection ? 0 : currentSize));
                contentBounds.center -= new Vector3(0, offset + currentSize / 2, 0);
                contentBounds.size = Vector3.zero;

                change = true;
            }

            if (viewBounds.min.y > contentBounds.max.y && itemTypeEnd > itemTypeStart)
            {
                float currentSize = contentBounds.size.y;
                float elementSize = (currentSize - contentSpacing * (currentLines - 1)) / currentLines;
                returnToTempPool(false, itemTypeEnd - itemTypeStart);
                itemTypeEnd = itemTypeStart;

                int offsetCount = Mathf.FloorToInt((viewBounds.min.y - contentBounds.max.y) / (elementSize + contentSpacing));
                if (totalCount >= 0 && itemTypeStart - offsetCount * contentConstraintCount < 0)
                {
                    offsetCount = Mathf.FloorToInt((float)(itemTypeStart) / contentConstraintCount);
                }
                itemTypeStart -= offsetCount * contentConstraintCount;
                if (totalCount >= 0)
                {
                    itemTypeStart = Mathf.Max(itemTypeStart, 0);
                }
                itemTypeEnd = itemTypeStart;

                float offset = offsetCount * (elementSize + contentSpacing);
                content.anchoredPosition -= new Vector2(0, offset + (reverseDirection ? 0 : currentSize));
                contentBounds.center -= new Vector3(0, offset + currentSize / 2, 0);
                contentBounds.size = Vector3.zero;

                change = true;
            }

            if (viewBounds.min.y > contentBounds.min.y + threshold)
            {
                float size = deleteItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.min.y > contentBounds.min.y + threshold + totalSize)
                {
                    size = deleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0) change = true;
            }

            if (viewBounds.max.y < contentBounds.max.y - threshold)
            {
                float size = deleteItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.max.y < contentBounds.max.y - threshold - totalSize)
                {
                    size = deleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0) change = true;
            }

            if (viewBounds.min.y < contentBounds.min.y)
            {
                float size = newItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.min.y < contentBounds.min.y - totalSize)
                {
                    size = newItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0) change = true;
            }

            if (viewBounds.max.y > contentBounds.max.y)
            {
                float size = newItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.max.y > contentBounds.max.y - totalSize)
                {
                    size = newItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0) change = true;
            }

            if (change) clearTempPool();

            return change;
        }
    }
}