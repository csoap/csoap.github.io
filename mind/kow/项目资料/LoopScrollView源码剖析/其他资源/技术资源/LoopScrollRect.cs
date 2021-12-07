using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Practice
{
    //为什么用UIBehaviour？
    //2019以后新出现的类，UGUI都最終继承自UIBehaviour，而UIBehaviour又继承自MonoBehaviour
    //这个类提供了UI刷新的相关方法，如 OnRectTransformDimensionsChange（），OnTransformParentChanged（）

    //为什么用bounds？
    //轴对齐的包围盒（简称 AABB）是与坐标轴对齐并且完全包围 某个对象的盒体。由于该盒体从不相对于这些轴旋转，因此仅通过其 center 和 extents 或者通过 min 和 max 点便可对它进行定义。
    //主要变量：
    //center:该包围盒的中心。    extents:该包围盒的范围。这始终是这些 Bounds 的 size 的一半。   size:该盒体的总大小。这始终是 extents 的两倍。
    //max:该盒体的最大点。这始终等于 center+extents。   min:该盒体的最小点。这始终等于 center-extents。
    //在x轴方向，大小等于max.x-min.x，y、z轴同理

    //计算的时候，没有用sizeDelta而是用了rect.size,为什么？
    //sizeDelta:此 RectTransform 相对于锚点之间距离的大小。如果锚点在一起，则 sizeDelta 与大小相同。如果锚点处于父项四个角的各个角中，则 sizeDelta 是矩形与其父项相比更大或更小的程度。
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class LoopScrollRect : UIBehaviour, IInitializePotentialDragHandler,IBeginDragHandler,IEndDragHandler,
        IDragHandler,IScrollHandler,ICanvasElement,ILayoutElement,ILayoutGroup
    {
        [Tooltip("Prefab Source")]//鼠标在这个字段上停留，可以看到提示
        public LoopScrollPrefabSource prefabSource;

        [Tooltip("Total count, negative means INFINITE mode")]
        public int totalCount;

        [Tooltip("Reverse direction for dragging")]
        public bool reverseDirection = false;

        [Tooltip("Rubber scale for outside")]
        public float rubberScale = 1;

        [HideInInspector]
        [System.NonSerialized]
        public LoopScrollDataSource dataSource = LoopScrollSendIndexSource.Instance;

        //感觉数据这么初始化，很奇怪，感觉硬要把两种结构揉到一起
        //可以这么改，在这里赋值的时候都是列表数据，处理数据的时候用接口实现多态，多行多列用整除就可以实现了
        //如果是不固定宽高的列表，大部分情况都是单行单列的
        public object[] objectsToFill
        {
            set
            {
                // wrapper for forward compatbility
                if (value != null)
                    dataSource = new LoopScrollArraySource<object>(value);
                else
                    dataSource = LoopScrollSendIndexSource.Instance;//感觉奇怪
            }
        }
        public RectTransform content { get { return m_Content; } set { m_Content = value; } }

        public enum MovementType
        {
            Unrestricted,//可以一直滑，滑得看不见内容都行。这个没必要，谁要滑到看不见
            Elastic,//滑到底后会弹回来
            Clamped//只能滑到底
        }

        public enum ScrollbarVisibility
        {
            Permanent,
            AutoHide,
            AutoHideAndExPandViewport
        }

        [Serializable]
        public class ScrollRectEvent : UnityEvent<Vector2> { }

        public bool horizontal { get { return m_Horizontal; } set { m_Horizontal = value; } }
        public bool veritical { get { return m_Vertical; } set { m_Vertical = value; } }
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }
        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }
        public bool inertia { get { return m_Inertia; } set { m_Inertia  = value; } }
        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }
        //viewport和m_viewRect区别是
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; } }

        public Scrollbar horizontalScrollbar
        {
            get { return m_HorizontalScrollbar; }
            set
            {
                if (m_HorizontalScrollbar)
                {
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(setHorizontalNormalizedPosition);
                }
                m_HorizontalScrollbar = value;
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(setHorizontalNormalizedPosition);
                setDirtyCaching();
            }
        }

        public void SrollToCell(int index,float speed)
        {
            if (totalCount >= 0 && (index < 0 || index >= totalCount))
            {
                Debug.LogErrorFormat("invalid index {0}", index);
            }
            StopAllCoroutines();
            if (speed <= 0)
            {
                RefillCells(index);
                return;
            }
            StartCoroutine(ScrollToCellCoroutine(index, speed));
        }

        IEnumerator ScrollToCellCoroutine(int index,float speed)
        {
            bool needMoving = true;
            while (needMoving)
            {
                yield return null;//wait until the next frame
                if (!m_Dragging)
                {
                    float move = 0;
                    if (index < itemTypeStart)
                    {
                        move = -Time.deltaTime * speed;
                    }
                    else if (index >= itemTypeEnd)
                    {
                        move = Time.deltaTime * speed;
                    }
                    else
                    {
                        m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                        var m_ItemBound = getBound4Item(index);
                        var offset = 0.0f;
                        if (direction == LoopScrollRectDirection.Vertical)
                        {
                            offset = reverseDirection ? (m_ViewBounds.min.y - m_ItemBound.min.y) : (m_ViewBounds.max.y - m_ItemBound.max.y);
                        }
                        else
                        {
                            offset = reverseDirection ? (m_ItemBound.max.x - m_ViewBounds.max.x) : (m_ItemBound.min.x - m_ViewBounds.min.y);
                        }
                        //check if we cannot move on
                        if (totalCount >= 0)
                        {
                            if (offset > 0 && itemTypeEnd == totalCount && !reverseDirection)
                            {
                                m_ItemBound = getBound4Item(totalCount - 1);
                                //reach bottom
                                if((direction==LoopScrollRectDirection.Vertical && m_ItemBound.min.y > m_ViewBounds.min.y)||
                                    (direction == LoopScrollRectDirection.Horizontal && m_ItemBound.max.x < m_ViewBounds.max.x))
                                {
                                    needMoving = false;
                                    break;
                                }
                            }
                            else if (offset < 0 && itemTypeStart == 0 && reverseDirection)
                            {
                                if ((direction == LoopScrollRectDirection.Vertical && m_ItemBound.max.y > m_ViewBounds.max.y) ||
                                    (direction == LoopScrollRectDirection.Horizontal && m_ItemBound.min.x < m_ViewBounds.min.x))
                                {
                                    needMoving = false;
                                    break;
                                }
                            }
                        }

                        float maxMove = Time.deltaTime * speed;
                        if (Mathf.Abs(offset) < maxMove)
                        {
                            needMoving = false;
                            move = offset;
                        }
                        else
                            offset = Mathf.Sign(offset) * maxMove;
                    }
                }
            }

            stopMovement();
            updatePrevData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset">从第offset开始填</param>
        /// <param name="fillViewRect"></param>
        public void RefillCells(int offset = 0, bool fillViewRect = false)
        {
            if (!Application.isPlaying || prefabSource == null) return;

            stopMovement();
            itemTypeStart = reverseDirection ? totalCount - offset : offset;
            if (totalCount >= 0 && itemTypeStart % contentConstraintCount != 0)
            {
                itemTypeStart = itemTypeStart / contentConstraintCount * contentConstraintCount;
            }
            itemTypeEnd = itemTypeStart;

            // Don't `Canvas.ForceUpdateCanvases();` here, or it will new/delete cells to change itemTypeStart/End
            returnToTempPool(reverseDirection, m_Content.childCount);

            float sizeToFill = Mathf.Abs(getDimension(viewRect.rect.size));//应该要填满的区域
            float sizeFilled = 0;//一定填满的
            float itemSize = 0;
            // m_ViewBounds may be not ready when RefillCells on Start

            while (sizeToFill > sizeToFill)
            {
                float size = reverseDirection ? newItemAtStart() : newItemAtEnd();
                if (size <= 0) break;
                itemSize = size;
                sizeFilled += size;
            }

            //怕上次没填满
            while (sizeToFill > sizeFilled)
            {
                float size = reverseDirection ? newItemAtStart() : newItemAtEnd();
                if (size <= 0) break;
                sizeFilled += size;
            }

            if (fillViewRect && itemSize > 0 && sizeFilled < sizeToFill)
            {
                int itemsToAddCount = (int)((sizeToFill - sizeFilled) / itemSize);//calculate how many items can be added above the offset, so it still is visible in the view
                int newOffset = offset - itemsToAddCount;
                if (newOffset < 0) newOffset = 0;
                if (newOffset != offset) RefillCells(newOffset);//refill again, with the new offset value, and now with fillViewRect disabled.
            }

            Vector2 pos = m_Content.anchoredPosition;
            if (direction == LoopScrollRectDirection.Vertical)
                pos.y = 0;
            else 
                pos.x = 0;
            m_Content.anchoredPosition = pos;
            m_ContentStartPosition = pos;

            clearTempPool();
            updateScrollbars(Vector2.zero);
        }

        public virtual void stopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                setNormalizedPosition(value.x, 0);
                setNormalizedPosition(value.y, 1);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected float threshold = 0;//这是啥？

        /// <summary>
        /// 列表里的数据从第几个开始显示，哪怕这个item只显示很小一部分，都算从这个item开始显示
        /// </summary>
        protected int itemTypeStart = 0;
        protected int itemTypeEnd = 0;

        /// <summary>
        /// 多行多列才有的组件
        /// </summary>
        protected GridLayoutGroup m_GridLayout = null;

        protected LoopScrollRectDirection direction = LoopScrollRectDirection.Horizontal;

        protected float contentSpacing
        {
            get
            {
                if (m_ContentSpaceInit) return m_ContentSpacing;
                m_ContentSpaceInit = true;
                m_ContentSpacing = 0;
                if (content != null)
                {
                    HorizontalOrVerticalLayoutGroup layout = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if (layout != null)
                    {
                        m_ContentSpacing = layout.spacing;
                    }
                    m_GridLayout = content.GetComponent<GridLayoutGroup>();
                    if (m_GridLayout != null)
                    {
                        m_ContentSpacing = Mathf.Abs(getDimension(m_GridLayout.spacing));
                    }
                }
                return m_ContentSpacing;
            }
        }

        /// <summary>
        /// 限定一行多少个，多行多列的在GridLayoutGroup里设置，非多行多列就默认1
        /// </summary>
        protected int contentConstraintCount
        {
            get
            {
                if (m_ContentConstraintCountInit)
                {
                    return m_ContentConstraintCount;
                }
                m_ContentConstraintCountInit = true;
                m_ContentConstraintCount = 1;
                if (content != null)
                {
                    GridLayoutGroup layout = content.GetComponent<GridLayoutGroup>();
                    if (layout != null)//只有多行多列才有
                    {
                        if (layout.constraint == GridLayoutGroup.Constraint.Flexible)
                        {
                            Debug.LogWarning("[LoopScrollRect] Flexible not supported yet");
                        }
                        //constraintCount:沿着约束轴应该有多少个单元格
                        m_ContentConstraintCount = layout.constraintCount;
                    }
                }
                return m_ContentConstraintCount;
            }
        }

        /// <summary>
        /// 当前开始显示的第多少行,多行多列的时候和itemTypeStart并不一样
        /// </summary>
        protected int startLine
        {
            get
            {
                return Mathf.CeilToInt((float)(itemTypeStart) / contentConstraintCount);
            }
        }

        /// <summary>
        /// 单行单列，正在显示共有多少行，这名字起得太让人误会了
        /// </summary>
        protected int currentLines
        {
            get
            {
                return Mathf.CeilToInt((float)(itemTypeEnd - itemTypeStart) / contentConstraintCount);
            }
        }

        /// <summary>
        /// 针对多行多列，一行多个的话，totalLines肯定比currentLines小
        /// </summary>
        protected int totalLines
        {
            get
            {
                return Mathf.CeilToInt((float)(totalCount) / contentConstraintCount);
            }
        }

        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null) { m_ViewRect = m_Viewport; }//这样赋值的话，这俩个不是一个吗？
                if (m_ViewRect == null) { m_ViewRect = (RectTransform)transform; }//意思是万一上衣行m_Viewport是空的话，补救一下
                return m_ViewRect;
            }
        }

        /// <summary>
        /// 记录一下将要更新的item
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <returns></returns>
        protected RectTransform getFromTempPool(int itemIndex)
        {
            RectTransform nextItem = null;
            if (deletedItemTypeStart > 0)
            {
                deletedItemTypeStart--;
                nextItem = content.GetChild(0) as RectTransform;
                //SetSiblingIndex->设置同级索引。 用于更改 GameObject 的同级索引。如果 GameObject 与其他 GameObject 共享一个父级并且处于同一级别（即它们共享相同的直接父级），
                //则这些 GameObject 被称为同级对象。同级索引显示每个 GameObject 在该同级层级视图中的位置。使用 SetSiblingIndex 更改 GameObject 在该层级视图中的位置。
                //当 GameObject 的同级索引发生更改时，其在“Hierarchy”窗口中的顺序也会改变。如果您需要更改 GameObject 子项的排序（例如使用布局组组件时），这很有用。
                nextItem.SetSiblingIndex(itemIndex - itemTypeStart + deletedItemTypeStart);
            }
            else if (deletedItemTypeEnd > 0)
            {
                deletedItemTypeEnd--;
                nextItem = content.GetChild(content.childCount - 1) as RectTransform;
                nextItem.SetSiblingIndex(itemIndex - itemTypeStart + deletedItemTypeStart);
            }
            else
            {
                nextItem = prefabSource.getObject().transform as RectTransform;
                nextItem.transform.SetParent(content, false);
                nextItem.gameObject.SetActive(true);
            }
            dataSource.provideData(nextItem, itemIndex);
            return nextItem;
        }

        /// <summary>
        /// 这里只是把原来要删的记号去掉
        /// </summary>
        /// <param name="fromStart"></param>
        /// <param name="count"></param>
        protected void returnToTempPool(bool fromStart,int count = 1)
        {
            if (fromStart) deletedItemTypeStart += count;
            else deletedItemTypeEnd += count;
        }

        protected void clearTempPool()
        {
            while (deletedItemTypeStart > 0)
            {
                deletedItemTypeStart--;
                prefabSource.returnObject(content.GetChild(0));
            }
            while (deletedItemTypeEnd > 0)
            {
                deletedItemTypeEnd--;
                prefabSource.returnObject(content.GetChild(content.childCount - 1));
            }
        }

        /// <summary>
        /// 在执行顺序的Update之后执行，如果想检查或控制某些东西是否在Update里完成了他的工作，可以使用LateUpdate
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (!m_Content) return;

            ensureLayoutHasRebuilt();
            updateScrollbarVisibility();
            updateBounds();

            float deltaTime = Time.deltaTime;
            Vector2 offset = calculateOffset(Vector2.zero);
            if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
            {
                Vector2 position = m_Content.anchoredPosition;
                for(int axis = 0; axis < 2; axis++)
                {
                    //Apply spring physics if movement is elastic and content has an offset from the view.
                    if (m_MovementType == MovementType.Elastic && offset[axis] != 0)
                    {
                        float speed = m_Velocity[axis];
                        //SmoothDamp:随着时间的推移，逐渐将值更改为期望的目标
                        position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis],
                            ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
                        m_Velocity[axis] = speed;
                    }
                    //else move content according to velocity with deceleration applied.
                    else if (m_Inertia)
                    {
                        m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                        if (Mathf.Abs(m_Velocity[axis]) < 1) m_Velocity[axis] = 0;
                        position[axis] += m_Velocity[axis] * deltaTime;
                    }
                    else
                    {
                        m_Velocity[axis] = 0;
                    }
                }

                if (m_Velocity != Vector2.zero)
                {
                    if (m_MovementType == MovementType.Clamped)
                    {
                        offset = calculateOffset(position - m_Content.anchoredPosition);
                        position += offset;
                    }
                    setContentAnchoredPosition(position);
                }
            }

            if (m_Dragging && m_Inertia)
            {
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
            }

            if (m_ViewBounds != m_prevViewBounds || m_ContentBounds != m_prevViewBounds || m_Content.anchoredPosition != m_PrevPosition)
            {
                updateScrollbars(offset);
                m_OnValueChanged.Invoke(normalizedPosition);
                updatePrevData();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        [SerializeField]
        private bool m_Horizontal = true;
        [SerializeField]
        private bool m_Vertical = true;
        [SerializeField]
        private RectTransform m_Content;
        [SerializeField]
        private MovementType m_MovementType = MovementType.Elastic;
        [SerializeField]
        private float m_Elasticity = 0.1f;// Only used for MovementType.Elastic
        [SerializeField]
        private bool m_Inertia = true;//惯性
        [SerializeField]
        private float m_DecelerationRate = 0.135f;// Only used when inertia is enabled
        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;
        [SerializeField]
        private RectTransform m_Viewport;
        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;
        [SerializeField]
        private Scrollbar m_VerticalScrollbar;
        [SerializeField]
        private ScrollbarVisibility m_HorizontalScrollbarVisibility;
        [SerializeField]
        private ScrollbarVisibility m_VerticalScrollbarVisibility;
        [SerializeField]
        private float m_HorizontalScrollbarSpacing;
        [SerializeField]
        private float m_verticalScrollbarSpacing;
        [SerializeField]
        private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();

        private bool m_ContentSpaceInit = false;
        private float m_ContentSpacing = 0;

        private bool m_ContentConstraintCountInit = false;
        private int m_ContentConstraintCount = 0;

        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        private Vector2 m_ContentStartPosition = Vector2.zero;//content开始滑动时，content的RectTransform 的轴心相对于锚点参考点的位置。

        private RectTransform m_ViewRect;//和m_Viewport区别在？

        //Bound：包围盒，边界框，AABB的简称，Mesh，Collider，Renderer都存在bound。（Mesh返回的是自身坐标，其余返回的是世界坐标）
        private Bounds m_ContentBounds;//计算容纳所有item的包围盒
        private Bounds m_ViewBounds;//

        private Vector2 m_Velocity;

        private bool m_Dragging;

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevContentBounds;
        private Bounds m_prevViewBounds;

        [NonSerialized]//不知道为什么特地标记成不能序列化
        private bool m_HasRebuiltLayout = false;

        private bool m_HSliderExpand;
        private bool m_VSliderExpand;
        private float m_HSliderHeight;
        private float m_VSliderWidth;

        [NonSerialized]
        private RectTransform m_Rect;

        private RectTransform m_HorizontalScrollbarRect;
        private RectTransform m_VerticalScrollbarRect;

        //DrivenRectTransformTracker:驱动的RectTransform的值由该组件控制，这些驱动值无法在检查器中编辑（显示为禁用）。保存场景时也不会保存它们，这会防止意外的场景文件更改。
        private DrivenRectTransformTracker m_Tracker;

        //readonly：动态常量，如果是一个引用类型，那么保存的是这个变量的内存地址，对这个引用的写操作是受限制的，但是对于这个变量里面的成员的读写操作是不受限制的。
        private readonly Vector3[] m_Corners = new Vector3[4];

        private int deletedItemTypeStart = 0;
        private int deletedItemTypeEnd = 0;

        //------------------------------------methods---------------------------------------//
        
        //总的来说，拖动鼠标这三个函数，如何减少误差这件事没有搞明白
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) { return; }

            //UIBehaviour.IsActive() -> 如果 GameObject 和 Component 处于激活状态，则返回 true。
            if (!IsActive()) return;

            updateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            //RectTransformUtility:Utility 类，包含用于 RectTransform 的 helper 方法。
            //ScreenPointToLocalPointInRectangle:将一个屏幕空间点转换为 RectTransform 的本地空间中位于其矩形平面上的一个位置。
            //m_PointerStartLocalCursor鼠标在viewrect上从哪开始点击
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_Dragging = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!IsActive()) return;

            Vector2 localCursor;
            //localCursor->拖的过程中，鼠标点在哪
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            updateBounds();

            Vector2 pointerDelta = localCursor - m_PointerStartLocalCursor;//鼠标滑动距离差
            Vector2 position = m_ContentStartPosition + pointerDelta;//content开始滑得位置+鼠标滑动差

            //偏移以将内容放置在视图中。
            Vector2 offset = calculateOffset(position - m_Content.anchoredPosition);//千万注意！m_Content.anchoredPosition 和 m_ContentStartPosition 的区别
            position += offset;
            if (m_MovementType == MovementType.Elastic)
            {
                if (offset.x != 0)
                {
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x) * rubberScale;
                }
                if (offset.y != 0)
                {
                    position.y=position.y- RubberDelta(offset.y, m_ViewBounds.size.y) * rubberScale;
                }
            }

            setContentAnchoredPosition(position);
        }



        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            m_Dragging = false;
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            m_Velocity = Vector2.zero;
        }

        public void OnScroll(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();

            if (m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = getBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_verticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = getBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (m_HSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = getBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_verticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        public void SetLayoutVertical()
        {
            updateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = getBounds();
        }

        public virtual void CalculateLayoutInputHorizontal() { }

        public virtual void CalculateLayoutInputVertical() { }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout) { updateCachedData(); }

            if (executing == CanvasUpdate.PostLayout)
            {
                updateBounds();
                updateScrollbars(Vector2.zero);
                updatePrevData();

                m_HasRebuiltLayout=true;
            }
        }

        /// <summary>
        /// 如何标准化位置呢？
        /// </summary>
        public float horizontalNormalizedPosition
        {
            get
            {
                updateBounds();
                if (totalCount > 0 && itemTypeEnd > itemTypeStart)
                {
                    float elementSize = (m_ContentBounds.size.x - contentSpacing * (currentLines - 1)) / currentLines;
                    float totalSize = elementSize * totalLines + contentSpacing * (totalLines - 1);
                    float offset = m_ContentBounds.min.x - elementSize * startLine - contentSpacing * startLine;

                    if (totalSize <= m_ViewBounds.size.x)
                    {
                        return (m_ViewBounds.min.x > offset) ? 1 : 0;
                    }
                    return (m_ViewBounds.min.x - offset) / (totalSize - m_ViewBounds.size.x);
                }
                else
                    return 0.5f;
            }
            set
            {
                setNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                updateBounds();
                if (totalCount > 0 && itemTypeEnd > itemTypeStart)
                {
                    float elementSize = (m_ContentBounds.size.y - contentSpacing * (currentLines - 1)) / currentLines;
                    float totalSize = elementSize * totalLines + contentSpacing * (totalLines - 1);
                    float offset = m_ContentBounds.max.y + elementSize * startLine + contentSpacing * startLine;

                    if (totalLines <= m_ViewBounds.size.y)
                    {
                        return (offset > m_ViewBounds.max.y) ? 1 : 0;
                    }
                    return (offset - m_ViewBounds.max.y) / (totalSize - m_ViewBounds.size.y);
                }
                else
                {
                    return 0.5f;
                }
            }
            set
            {
                setNormalizedPosition(value, 1);
            }
        }

        public virtual void LayoutComplete() { }

        public virtual void GraphicUpdateComplete() { }

        public virtual float minWidth { get { return -1; } }

        public virtual float preferredWidth { get { return -1; } }

        public virtual float flexibleWidth { get; private set; }

        public virtual float minHeight { get { return -1; } }

        public virtual float preferredHeight { get { return -1; } }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return -1; } }


        //////////////////////////-----------protected---------//////////////////////////////

#if UNITY_EDITOR
        protected override void Awake()
        {
            base.Awake();
            if (Application.isPlaying)
            {
                float value = (reverseDirection ^ (direction == LoopScrollRectDirection.Horizontal)) ? 0 : 1;
                Debug.Assert(Mathf.Abs(getDimension(content.pivot)) == value, this);
                Debug.Assert(Mathf.Abs(getDimension(content.anchorMin)) == value, this);
                Debug.Assert(Mathf.Abs(getDimension(content.anchorMax)) == value, this);
            }
        }
#endif

        //获得item的宽高
        protected abstract float getSize(RectTransform item);//abstract：在派生类中一定要有抽象方法的具体实现。
        //dimensin在英语里的意思是尺寸的意思，但是这里是求分量y，简直无语
        protected abstract float getDimension(Vector2 vector2);
        //将传入数据包装成 x->0,y->value vector
        protected abstract Vector2 getVector(float value);

        protected enum LoopScrollRectDirection
        {
            Vertical,
            Horizontal
        }

        protected virtual bool UpdateItems(Bounds viewBounds,Bounds contentBounds) { return false; }

        protected void setDirty()
        {
            if (!IsActive()) { return; }
            //LayoutRebuilder:封装器类，用于管理 CanvasElement 的布局重新构建。
            //MarkLayoutForRebuild:将给定的 RectTransform 标记为需要在下一布局过程中重新计算其布局。
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected void setDirtyCaching()
        {
            if (!IsActive()) { return; }
            //CanvasUpdateRegistry:CanvasElements 可以在其中注册自己以便重新构建的位置。
            //RegisterCanvasElementForLayoutRebuild:重新构建给定元素的布局。
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        /// <summary>
        /// 更新content的anchoredPosition
        /// </summary>
        /// <param name="position"></param>
        protected virtual void setContentAnchoredPosition(Vector2 position)
        {
            if (!m_Horizontal) position.x = m_Content.anchoredPosition.x;
            if (!m_Vertical) position.y = m_Content.anchoredPosition.y;
            if ((position - m_Content.anchoredPosition).sqrMagnitude > 0.0001f)
            {
                m_Content.anchoredPosition = position;
                updateBounds(true);
            }
        }

        protected float deleteItemAtStart()
        {
            // special case: when moving or dragging, we cannot simply delete start when we've reached the end
            if ((m_Dragging || m_Velocity != Vector2.zero) && totalCount >= 0 && itemTypeStart < contentConstraintCount) { return 0; }

            int availableChilds = content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
            Debug.Assert(availableChilds >= 0);//断言某个条件，并在失败时将错误消息记录到 Unity 控制台。
            if (availableChilds == 0)
            { return 0; }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(deletedItemTypeStart) as RectTransform;
                size = Mathf.Max(getSize(oldItem), size);
                returnToTempPool(true);
                availableChilds--;
                itemTypeStart++;
                if (availableChilds == 0) { break; }//Just delete the whole row
            }

            if (!reverseDirection)
            {
                Vector2 offset = getVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }
            return size;
        }

        /// <summary>
        /// 列表尾删除一行item，并返回这一行的宽高
        /// </summary>
        /// <returns></returns>
        protected float deleteItemAtEnd()
        {
            if ((m_Dragging || m_Velocity != Vector2.zero) && totalCount >= 0 && itemTypeStart < contentConstraintCount) { return 0; }

            int availableChilds = content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
            Debug.Assert(availableChilds >= 0);//断言某个条件，并在失败时将错误消息记录到 Unity 控制台。
            if (availableChilds==0)
            { return 0; }

            float size = 0;
            for(int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(content.childCount - deletedItemTypeEnd - 1) as RectTransform;
                size = Mathf.Max(getSize(oldItem), size);
                returnToTempPool(false);
                availableChilds--;
                itemTypeEnd--;
                if (itemTypeEnd % contentConstraintCount == 0 || availableChilds == 0) { break; }//Just delete the whole row
            }

            if (reverseDirection)
            {
                Vector2 offset = getVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }
            return size;
        }

        protected float newItemAtStart()
        {
            if (totalCount >= 0 && itemTypeStart - contentConstraintCount < 0) return 0;

            float size = 0;
            for(int i = 0; i < contentConstraintCount; i++)
            {
                itemTypeStart--;
                RectTransform newItem = getFromTempPool(itemTypeStart);
                newItem.SetSiblingIndex(deletedItemTypeStart);
                size = Mathf.Max(getSize(newItem), size);
            }
            threshold = Mathf.Max(threshold, size * 1.5f);

            if (!reverseDirection)
            {
                Vector2 offset = getVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }

            return size;
        }

        protected float newItemAtEnd()
        {
            if (totalCount >= 0 && itemTypeEnd >= totalCount) return 0;
            float size = 0;

            //issue 4:fill lines to end first
            int availableChilds = content.childCount - deletedItemTypeStart - deletedItemTypeEnd;
            int count = contentConstraintCount - (availableChilds % contentConstraintCount);
            for(int i = 0; i < count; i++)
            {
                RectTransform newItem = getFromTempPool(itemTypeEnd);
                newItem.SetSiblingIndex(content.childCount - deletedItemTypeEnd - 1);
                size = Mathf.Max(getSize(newItem), size);
                itemTypeEnd++;
                if (totalCount >= 0 && itemTypeEnd >= totalCount)
                {
                    break;
                }
            }
            threshold = Mathf.Max(threshold, size * 1.5f);

            if (reverseDirection)
            {
                Vector2 offset = getVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }
            return size;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_HorizontalScrollbar)
            {
                m_HorizontalScrollbar.onValueChanged.AddListener(setHorizontalNormalizedPosition);
            }
            if (m_VerticalScrollbar)
            {
                m_VerticalScrollbar.onValueChanged.AddListener(setVerticalNormalizedPosition);
            }

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(setHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(setVerticalNormalizedPosition);

            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

            base.OnDisable();
        }
        /////////////////////////////------------private----------/////////////////////////////

        /// <summary>
        /// 在moveType==Elastic的情况下，这在干嘛？
        /// 从函数名来看是要抹去传入两个变量的差，不明白为什么这么做
        /// </summary>
        /// <param name="overStretching"></param>
        /// <param name="viewSize"></param>
        /// <returns></returns>
        private static float RubberDelta(float overStretching,float viewSize)
        {
            //Mathf.Sign-> 返回 f 的符号。当 f 为正数或零时，返回值为 1，当 f 为负数时，返回值为 - 1。
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        private bool hScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                return true;
            }
        }

        private bool vScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                return true;
            }
        }

        private void setHorizontalNormalizedPosition(float value)
        {
            setNormalizedPosition(value, 0);
        }

        private void setVerticalNormalizedPosition(float value)
        {
            setNormalizedPosition(value, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="axis"></param>
        private void setNormalizedPosition(float value, int axis)
        {
            //LoopScrollRect
            if (totalCount <= 0 || itemTypeEnd <= itemTypeStart)
                return;

            ensureLayoutHasRebuilt();
            updateBounds();

            Vector3 localPosition = m_Content.localPosition;
            float newLocalPosition = localPosition[axis];
            if (axis == 0)//水平
            {
                float elementSize = (m_ContentBounds.size.x - contentSpacing * (currentLines - 1)) / currentLines;//一个item高度=总高/共多少行
                float totalSize = elementSize * totalLines + contentSpacing * (totalLines - 1);
                float offset = m_ContentBounds.min.x - elementSize * startLine - contentSpacing * startLine;//这算的是什么
                //这个怎么算的还不清楚
                newLocalPosition += m_ViewBounds.min.x - value * (totalSize - m_ViewBounds.size[axis])- offset;
            }
            else if (axis == 1)
            {
                float elementSize = (m_ContentBounds.size.y - contentSpacing * (currentLines - 1)) / currentLines;
                float totalSize = elementSize * totalLines + contentSpacing * (totalLines - 1);
                float offset = m_ContentBounds.max.y + elementSize * startLine + contentSpacing * startLine;

                newLocalPosition -= offset - value * (totalSize - m_ViewBounds.size.y) - m_ViewBounds.max.y;
            }

            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                m_Velocity[axis] = 0;
                updateBounds(true);
            }
        }

        private void ensureLayoutHasRebuilt()
        {
            //CanvasUpdateRegistry:CanvasElements 可以在其中注册自己以便重新构建的位置。
            //IsRebuildingLayout:是否正在重新布局
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        /// <summary>
        /// viewBounds始终不变
        /// 每次滑动的时候，修正content包围盒大小，确保content比view大
        /// </summary>
        /// <param name="updateItems"></param>
        private void updateBounds(bool updateItems=false)
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = getBounds();

            if (m_Content == null) return;

            // Don't do this in Rebuild
            if (Application.isPlaying && updateItems && UpdateItems(m_ViewBounds, m_ContentBounds))
            {
                Canvas.ForceUpdateCanvases();
                m_ContentBounds = getBounds();
            }

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            Vector3 excess = m_ViewBounds.size - contentSize;//要是小的话后面就减去
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (m_Content.pivot.x - 0.5f);
                contentSize.x = m_ViewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (m_Content.pivot.y - 0.5f);
                contentSize.y = m_ViewBounds.size.y;
            }

            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;
        }

        /// <summary>
        /// 计算contentBounds最大边界
        /// 有兴趣可以记住这个计算过程，没啥好解释的
        /// </summary>
        /// <returns></returns>
        private Bounds getBounds()
        {
            if (m_Content == null)
            {
                return new Bounds();
            }

            Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            //为什么要把世界矩阵转化为本地矩阵？
            //选择合适坐标系是为了简化计算，一般移动是先转化为本地坐标，移动之后再转化为世界坐标
            Matrix4x4 toLocal = viewport.worldToLocalMatrix;
            //GetWorldCorners:获取世界空间中计算出的矩形的角。顺序：0-左下，1-左上，2-右上，3-右下
            //Each corner provides its world space value. The returned array of 4 vertices is clockwise. 
            //It starts bottom left and rotates to top left, then top right, and finally bottom right.
            //Note that bottom left, for example, is an (x, y, z) vector with x being left and y being bottom.
            m_Content.GetWorldCorners(m_Corners);

            for(int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            Bounds bounds = new Bounds(vMin, Vector3.zero);
            //Encapsulate:增长边界以包括该点。
            bounds.Encapsulate(vMax);
            return bounds;
        }

        /// <summary>
        /// 返回第index个item的bounds
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Bounds getBound4Item(int index)
        {
            if (m_Content == null) return new Bounds();

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = viewRect.worldToLocalMatrix;
            int offset = index - itemTypeStart;
            if (offset < 0 || offset >= m_Content.childCount) return new Bounds();
            var rt = m_Content.GetChild(offset) as RectTransform;
            if (rt == null) return new Bounds();
            rt.GetWorldCorners(m_Corners);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            Bounds bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        /// <summary>
        /// 计算content和view的bounds的差额
        /// </summary>
        /// <param name="delta">content滑动了多少</param>
        /// <returns></returns>
        private Vector2 calculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (m_MovementType == MovementType.Unrestricted) { return offset; }
            if (m_MovementType == MovementType.Clamped)
            {
                if (totalCount < 0) return offset;
                if (getDimension(delta) < 0 && itemTypeStart > 0) { return offset; }
                if (getDimension(delta) > 0 && itemTypeEnd < totalCount)
                {
                    return offset;
                }
            }

            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;

            if (m_Horizontal)
            {
                min.x += delta.x;//把content扩大至偏移
                max.x += delta.x;
                if (min.x > m_ViewBounds.min.x)//这是在修正什么？
                    offset.x = m_ViewBounds.min.x - min.x;
                else if (max.x < m_ViewBounds.max.x)//view的max可能也是要小于content的max
                    offset.x = m_ViewBounds.max.x - max.x;
            }

            if (m_Vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
                if (min.y < m_ViewBounds.min.y)
                    offset.y = m_ViewBounds.min.y - min.y;
                else if (max.y > m_ViewBounds.max.y)
                    offset.y = m_ViewBounds.max.y - max.y;
            }

            return offset;
        }

        private void updateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                if (m_ContentBounds.size.x > 0 && totalCount > 0)
                {
                    float elementSize = (m_ContentBounds.size.x - contentSpacing * (currentLines - 1)) / currentLines;
                    float totalSize = elementSize * totalLines + contentSpacing * (totalLines - 1);
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / totalSize);
                }
                else
                {
                    m_HorizontalScrollbar.size = 1;
                }
                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                if (m_ContentBounds.size.y > 0 && totalCount > 0)
                {
                    float elementSize = (m_ContentBounds.size.y - contentSpacing * (currentLines - 1)) / currentLines;
                    float totalSize = elementSize * totalLines + contentSpacing * (totalLines - 1);
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / totalSize);
                }
                else
                {
                    m_VerticalScrollbar.size = 1;
                }
                m_VerticalScrollbar.value = 1;
            }
        }

        /// <summary>
        /// 名字真绝了，把当前滑动的bounds数据记录下来
        /// </summary>
        private void updatePrevData()
        {
            if (m_Content == null)
            {
                m_PrevPosition = Vector2.zero;
            }
            else
            {
                m_PrevPosition = m_Content.anchoredPosition;
            }
            m_prevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        /// <summary>
        /// 名字起得令人费解，还以为跟数据有关，谁知道是滚动条相关
        /// </summary>
        private void updateCachedData()
        {
            Transform transform = this.transform;
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = viewRect.parent == transform;
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && m_HorizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExPandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && m_VerticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExPandViewport;
            m_HSliderHeight = (m_HorizontalScrollbarRect == null) ? 0 : m_HorizontalScrollbarRect.rect.height;
            m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        private void updateScrollbarVisibility()
        {
            if (m_VerticalScrollbar && m_VerticalScrollbarVisibility != ScrollbarVisibility.Permanent &&
                m_VerticalScrollbar.gameObject.activeSelf != vScrollingNeeded)
            {
                m_VerticalScrollbar.gameObject.SetActive(vScrollingNeeded);
            }

            if (m_HorizontalScrollbar && m_HorizontalScrollbarVisibility != ScrollbarVisibility.Permanent &&
                m_HorizontalScrollbar.gameObject.activeSelf != hScrollingNeeded)
            {
                m_HorizontalScrollbar.gameObject.SetActive(hScrollingNeeded);
            }
        }

        private void updateScrollbarLayout()
        {
            if (m_VSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX | 
                    DrivenTransformProperties.AnchorMaxX | 
                    DrivenTransformProperties.SizeDeltaX | 
                    DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_verticalScrollbarSpacing),
                        m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.y, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.y, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x,
                        -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
            }
        }
    }
}