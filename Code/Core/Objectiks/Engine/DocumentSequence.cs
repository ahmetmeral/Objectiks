using Newtonsoft.Json;
using Objectiks.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Objectiks.Engine
{
    public class SequenceResult
    {
        public object Value { get; set; }
        public bool IsNew { get; set; }
    }

    public class DocumentSequence
    {
        public string TypeOf { get; set; }
        public object Sequence { get; set; }
        public DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public bool Exists { get; set; }

        public DocumentSequence(string typeOf, object sequence)
        {
            TypeOf = typeOf;
            Sequence = sequence;
            UpdatedAt = DateTime.UtcNow;
            Exists = true;
        }

        private object GetNewSequenceId(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            if (typeCode == TypeCode.Int32)
            {
                int seq = 0;
                Int32.TryParse(Sequence.ToString(), out seq);
                Sequence = Interlocked.Increment(ref seq);
            }
            else if (typeCode == TypeCode.Int64)
            {
                long seq = 0;
                Int64.TryParse(Sequence.ToString(), out seq);
                Sequence = Interlocked.Increment(ref seq);
            }
            else if (typeCode == TypeCode.String || typeCode == TypeCode.Object)
            {
                Sequence = Guid.NewGuid();
            }
            else
            {
                throw new Exception($"Undefined sequence type {typeCode}");
            }

            return Sequence;
        }

        public SequenceResult GetTypeOfSequence(Type type, object currentValue)
        {
            var primary = new SequenceResult();

            if (currentValue == null)
            {
                primary.Value = GetNewSequenceId(type);
                primary.IsNew = true;

                return primary;
            }

            TypeCode typeCode = Type.GetTypeCode(type);

            if (typeCode == TypeCode.Int32)
            {
                int.TryParse(currentValue.ToString(), out int current);

                if (current == 0)
                {
                    int seq = 0;
                    Int32.TryParse(Sequence.ToString(), out seq);
                    Sequence = Interlocked.Increment(ref seq);
                    primary.Value = Sequence;
                    primary.IsNew = true;
                }
            }
            else if (typeCode == TypeCode.Int64)
            {
                Int64.TryParse(currentValue.ToString(), out long current);

                if (current == 0)
                {
                    long seq = 0;
                    Int64.TryParse(Sequence.ToString(), out seq);
                    Sequence = Interlocked.Increment(ref seq);
                    primary.Value = Sequence;
                    primary.IsNew = true;
                }
            }
            else if (typeCode == TypeCode.String || typeCode == TypeCode.Object)
            {
                if (String.IsNullOrWhiteSpace(currentValue.ToString()))
                {
                    Sequence = Guid.NewGuid();
                    primary.Value = Sequence;
                    primary.IsNew = true;
                }
            }
            else
            {
                throw new Exception($"Undefined sequence type {typeCode}");
            }

            return primary;
        }
    }
}
