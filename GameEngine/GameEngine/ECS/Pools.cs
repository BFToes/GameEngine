using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace ECS.Pool
{
    /// <summary>
    /// an item that can be stored in a pool.
    /// used so that the pool item has a reference to its index in the array.
    /// </summary>
    public interface IPoolItem
    {
        int ID { get; set; }
    }
    /// <summary>
    /// a simple collection to be used as a non local grow array
    /// </summary>
    public interface IPool
    {
        public IPoolItem this[int index] { get; set; }
    }


    public interface IBundlePool : IPool
    {
        /// <summary>
        /// creates a new array of size <paramref name="length"/>.
        /// </summary>
        public void Initiate(int length);
        /// <summary>
        /// swaps the end of the array with the given index. then removes the end. 
        /// </summary>
        /// <param name="FreeIndex"></param>
        public void Replace(int index, int length);
        /// <summary>
        /// Resizes the array to the specified size.
        /// </summary>
        /// <param name="newSize"></param>
        public void Resize(int newSize);
        /// <summary>
        /// Clears the last item in the pool.
        /// </summary>
        public void Clear(int index);
    }
    public class BundlePool<T> : IPool, IBundlePool where T : IPoolItem
    {
        private T[] _array;
        IPoolItem IPool.this[int index]
        {
            get => _array[index];
            set => _array[index] = (T)value;
        }
        public T this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        void IBundlePool.Initiate(int length) => _array = new T[length];
        void IBundlePool.Replace(int index, int length) => _array[index] = _array[length];
        void IBundlePool.Resize(int newSize) => Array.Resize(ref _array, newSize);
        void IBundlePool.Clear(int index) => _array[index] = default;

        public IEnumerator<T> GetEnumerator(int Length)
        {
            for (int i = 0; i < Length; i++)
                yield return _array[i];
        }
    }
    public class SinglePool<T> : IPool where T : IPoolItem
    {
        private T[] _array;
        public int Length { get; private set; }

        IPoolItem IPool.this[int index]
        {
            get => _array[index];
            set => _array[index] = (T)value;
        }

        /// <summary>
        /// adds <paramref name="Item"/> to <see cref="Pool{T}"/>
        /// </summary>
        public int Add(T Item)
        {
            if (Length == _array.Length) // if too small
                Array.Resize(ref _array, Length << 1); // make bigga
            _array[Length] = Item; // add to end
            return Length++;
        }
        /// <summary>
        /// removes <paramref name="Item"/> from <see cref="Pool{T}"/>
        /// </summary>
        public void Remove(int Index)
        {
            _array[Index].ID = -1; // does not appear in any pool anymore
            if (Index != --Length) // if not at the end
            {
                _array[Index] = _array[Length]; // replace removed value with end value
                _array[Index].ID = Index; // update ID
            }
            _array[Length] = default; // clear end

            // resize if neccessary
            int newSize = _array.Length >> 1;
            if (Length < newSize)
                Array.Resize(ref _array, newSize);
        }
        /// <summary>
        /// clears all items in <see cref="Pool{T}"/>
        /// </summary>
        public void Clear()
        {
            Array.Clear(_array, 0, Length);
            Length = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
                yield return _array[i];
        }
    }

    /// <summary>
    /// A container of multiple Pools
    /// </summary>
    public class BundlePoolContainer
    {
        private const int DEFAULT_ARRAY_SIZE = 128;

        public readonly Type[] Types;
        public int Length { get; private set; }

        private readonly IBundlePool[] _pools;
        private int _arraySize;

        public IPoolItem this[int poolIndex, int itemIndex]
        {
            get => _pools[poolIndex][itemIndex];
            protected set
            {
                _pools[poolIndex][itemIndex] = value;
                value.ID = itemIndex;
            }
        }
        public IPool this[int poolIndex]
        {
            get => _pools[poolIndex];
        }

        public BundlePoolContainer(IEnumerable<Type> Types, int DefaultLength = DEFAULT_ARRAY_SIZE)
        {
            this.Types = Types.ToArray();
            this._arraySize = DefaultLength;
            this._pools = new IBundlePool[this.Types.Length];


            for (int i = 0; i < this.Types.Length; i++)
            {
                // instance pool Array
                _pools[i] = (IBundlePool)Activator.CreateInstance(typeof(BundlePool<>).MakeGenericType(this.Types[i]));
                _pools[i].Initiate(_arraySize);
            }

            Length = 0;
        }

        public BundlePool<T> GetPool<T>() where T : IPoolItem => (BundlePool<T>)_pools[FindType<T>()];
        public T GetItem<T>(int index) where T : IPoolItem => (T)this[FindType<T>(), index];

        protected void SetLayer(List<IPoolItem> Items, int Index, params bool[] Mask)
        {
            int j = 0;
            for (int i = 0; i < _pools.Length; i++)
            {
                if (Mask.Length >= i || Mask[i])
                {
                    _pools[i][Index] = Items[j++];
                    _pools[i][Index].ID = Index;
                }
            }
        }
        /// <summary>
        /// creates an empty slot in each pool to be filled
        /// </summary>
        protected int AddLayer(params bool[] Mask)
        {
            if (Length == _arraySize)
            {
                _arraySize = _arraySize << 1;
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i].Resize(_arraySize);
            }

            for (int i = 0; i < _pools.Length; i++)
            {
                if (Mask.Length >= i || Mask[i])
                {
                    _pools[i][Length] = (IPoolItem)Activator.CreateInstance(Types[i]);
                    _pools[i][Length].ID = _arraySize;
                }
            }

            return Length++;
        }
        /// <summary>
        /// removes a item from each pool at <paramref name="itemIndex"/>
        /// </summary>
        protected void RemoveLayer(int itemIndex)
        {
            if (itemIndex != --Length) // if not at the end
            {
                for (int i = 0; i < _pools.Length; i++)
                {
                    _pools[i][itemIndex] = _pools[i][Length]; // replace removed value with end value
                    _pools[i][itemIndex].ID = itemIndex; // update ID of moved value
                }
            }
            for (int i = 0; i < _pools.Length; i++)
                _pools[i][Length] = default; // clear end

            // resize if neccessary
            int newSize = _arraySize >> 1;
            if (Length < newSize)
                for (int i = 0; i < _pools.Length; i++)
                    _pools[i].Resize(newSize);
        }
        /// <summary>
        /// returns the item in each pool at this index.
        /// </summary>
        protected IEnumerable<IPoolItem> GetLayer(int itemIndex, params bool[] Mask)
        {
            for (int i = 0; i < _pools.Length; i++)
                if (Mask.Length >= i || Mask[i])
                    yield return _pools[i][itemIndex];
        }
        public IEnumerator<T> GetEnumerator<T>() where T : IPoolItem
        {
            int poolIndex = FindType<T>();
            return ((BundlePool<T>)_pools[poolIndex]).GetEnumerator(Length);
        }

        /// <summary>
        /// finds the index of the pool of type <typeparamref name="Type"/>
        /// </summary>
        public int FindType<Type>() => Array.BinarySearch(Types, typeof(Type));
        /// <summary>
        /// finds the index of the pool of type <typeparamref name="Type"/>
        /// </summary>
        public int FindType(Type T) => Array.BinarySearch(Types, T);
    }
}
