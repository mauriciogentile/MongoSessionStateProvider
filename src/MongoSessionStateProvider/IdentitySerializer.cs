using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace AspNet.Session.MongoSessionStateProvider
{
    public class IdentitySerializer : BsonBaseSerializer
    {
        private static readonly IdentitySerializer instance = new IdentitySerializer();

        public static IdentitySerializer Instance
        {
            get
            {
                return instance;
            }
        }

        public override object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            switch (bsonReader.CurrentBsonType)
            {
                case BsonType.ObjectId:
                    return bsonReader.ReadObjectId();

                case BsonType.String:
                    return new ObjectId(bsonReader.ReadString());

                case BsonType.Null:
                    return null;

                default:
                    throw new FormatException(string.Format("Cannot deserialize Identity from BsonType: {0}", bsonReader.CurrentBsonType));
            }
        }

        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            return Deserialize(bsonReader, nominalType, options);
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            var identity = (ObjectId)value;
            BsonType bsonType = options == null ? BsonType.ObjectId : ((RepresentationSerializationOptions)options).Representation;
            switch (bsonType)
            {
                case BsonType.String:
                    bsonWriter.WriteString(identity.ToString());
                    break;
                case BsonType.ObjectId:
                    bsonWriter.WriteObjectId(identity);
                    break;
                default:
                    throw new BsonSerializationException(string.Format("'{0}' is not a valid representation for type 'Identity'", bsonType));
            }
        }
    }
}
