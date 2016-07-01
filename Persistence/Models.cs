using System;
using System.Collections.Generic;
using System.Linq;
using DataAttribute = amita.primitives.net.Attribute;

namespace Persistence
{
    public abstract class EFAttribute
    {
        public virtual int ID { get; set; }
        public virtual string name { get; set; }
    }

    public class ValueEFAttribute : EFAttribute
    {
        public virtual string value { get; set; }
    }

    public class CollectionEFAttribute : EFAttribute
    {
        public virtual ICollection<EFAttribute> attributes { get; set; } = new HashSet<EFAttribute>();
    }

    public static class AttributeMappers
    {
        public static EFAttribute ToEFAttribute(this DataAttribute source)
        {
            return
            source
                .match(
                    s => {
                        return new ValueEFAttribute()
                        {
                            name = source.name,
                            value = s
                        } as EFAttribute;
                    },
                    c => {
                        var col_attribute =
                        new CollectionEFAttribute()
                        {
                            name = source.name
                        };

                        c.Select(ToEFAttribute)
                        .ToList()
                        .ForEach(i => col_attribute.attributes.Add(i));


                        return col_attribute as EFAttribute;
                    }
                );
        }

        public static DataAttribute ToDataAttribute(this EFAttribute source)
        {
            return
            source
                .match(
                    v => {
                        return DataAttribute.create_value(v.name, v.value);
                    },
                    c => {
                        return DataAttribute.create_collection(c.name, c.attributes.Select(ToDataAttribute));
                    }
                );
        }

        public static P match<P>
                        (this EFAttribute source
                        , Func<ValueEFAttribute, P> value
                        , Func<CollectionEFAttribute, P> collection)
        {

            if (source is ValueEFAttribute)
            {
                return value(source as ValueEFAttribute);
            }

            if (source is CollectionEFAttribute)
            {
                return collection(source as CollectionEFAttribute);
            }


            throw new Exception("Unexpected case");
        }
    }

}
