namespace FindInStones
{
    public static class SortingOrderManager
    {
        const int ItemSortingOrder = 0;
        const int InitialStoneOrder = 1;

        static int _nextOrder = InitialStoneOrder;

        public static int ItemOrder => ItemSortingOrder;

        public static int GetNext()
        {
            return _nextOrder++;
        }

        public static int BringToFront(UnityEngine.SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
                return _nextOrder - 1;

            spriteRenderer.sortingOrder = GetNext();
            return spriteRenderer.sortingOrder;
        }

        public static void Reset()
        {
            _nextOrder = InitialStoneOrder;
        }
    }
}
