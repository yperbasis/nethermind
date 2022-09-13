using FASTER.core;
using Nethermind.Core;

namespace Nethermind.Db.Faster
{
    public class DbOnFaster : IDb
    {

        private readonly FasterKV<byte[], byte[]> _db;
        private readonly SimpleFunctions<byte[], byte[]> _simpleFunc = new();
        private readonly HashSet<IBatch> _currentBatches = new();
        private bool _isDisposed;

        public DbOnFaster()
        {
            var log = Devices.CreateLogDevice("c:/temp/faster/hlog.log");
            var objlog = Devices.CreateLogDevice("c:/temp/faster/hlog.obj.log");
            var settings = new FasterKVSettings<byte[], byte[]>("c:/temp/faster")
            {
                LogDevice = log,
                ObjectLogDevice = objlog,
            };
            _db = new(settings);
        }

        public byte[]? this[byte[] key]
        {
            get
            {
                using var session = _db.NewSession(_simpleFunc);
                return session.Read(key).output;
            }
            set
            {
                using var session = _db.NewSession(_simpleFunc);
                if (value == null)
                {
                    session.Delete(ref key);
                }
                else
                {
                    session.Upsert(key, value);
                }
            }
        }

        public KeyValuePair<byte[], byte[]?>[] this[byte[][] keys] =>
            throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            _db.Dispose();
        }

        public void Flush()
        {
            _db.Log.FlushAndEvict(true);
        }

        public IEnumerable<KeyValuePair<byte[], byte[]>> GetAll(bool ordered = false)
        {
            if (ordered)
                throw new NotImplementedException("Faster does not do ordered");
            using var session = _db.NewSession(_simpleFunc);
            using var iterator = session.Iterate();
            while (iterator.GetNext(out RecordInfo record, out byte[]? key, out byte[]? value))
            {
                if (key != null)
                    yield return new KeyValuePair<byte[], byte[]>(key, value);
            } 
        }

        public IEnumerable<byte[]> GetAllValues(bool ordered = false)
        {
            if (ordered)
                throw new NotImplementedException("Faster does not do ordered");
            using var session = _db.NewSession(_simpleFunc);
            using var iterator = session.Iterate();
            while (iterator.GetNext(out RecordInfo record, out byte[]? key, out byte[]? value))
            {
                if (key != null)
                    yield return value;
            } 
        }

        public bool KeyExists(byte[] key)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException($"Attempted to read form a disposed database {Name}");
            }
            return this[key] != null;
        }

        public void Remove(byte[] key)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException($"Attempted to delete form a disposed database {Name}");
            }
            this[key] = null;
        }

        public IBatch StartBatch()
        {
            var batch = new FasterBatch(this);
            _currentBatches.Add(batch);
            return batch;
        }

        internal ClientSession<byte[], byte[], byte[], byte[], Empty, IFunctions<byte[], byte[], byte[], byte[], Empty>>? NewSession(SimpleFunctions<byte[], byte[]> simpleFunctions)
        {
            return _db.NewSession(_simpleFunc);
        }

        public class FasterBatch : IBatch
        {
            private DbOnFaster _db;
            private ClientSession<byte[], byte[], byte[], byte[], Empty, IFunctions<byte[], byte[], byte[], byte[], Empty>> _session;

            public FasterBatch(DbOnFaster db)
            {
                _db = db;
                _session = db.NewSession(new SimpleFunctions<byte[], byte[]>());
            }

            public byte[]? this[byte[] key]
            {
                get
                {
                    return _session.Read(key).output;
                }
                set
                {
                    if (value == null)
                    {
                        _session.Delete(ref key);
                    }
                    else
                    {
                        _session.Upsert(key, value);
                    }
                }
            }

            public void Dispose()
            {
                if (_db._isDisposed)
                    _session?.Dispose();
            }
        }
    }
}
