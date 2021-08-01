using System;
using System.Collections.Generic;
using BinarySerializer.Data;
using BinarySerializer.Serializers;
using BinarySerializer.Serializers.Baselines;
using Serializer = BinarySerializer.BinarySerializer;
using GameEngine.ECS.Systems;



// https://github.com/voledyhil/Mini
namespace GameEngine.ECS
{
    public partial class World
    {
        /// <summary>
        /// Reads data from DataReader, and updates the world of entities.
        /// </summary>
        /// <param name="data">data</param>
        public void ReadFrom(byte[] data)
        {
            using (BinaryDataReader reader = new BinaryDataReader(data))
            {
                while (reader.Position < reader.Length)
                {
                    uint key = reader.ReadUInt();
                    if (key == uint.MaxValue) break;

                    //if (!_entities.TryGetValue(key, out Entity entity)) // if byte
                    //   entity = CreateEntity(key);

                    using (BinaryDataReader entityReader = reader.ReadNode())
                    {
                        while (entityReader.Position < entityReader.Length)
                        {
                            byte index = entityReader.ReadByte();
                            if (index == byte.MaxValue) break;

                            using (BinaryDataReader componentReader = entityReader.ReadNode())
                            {
                                IComponent component;
                                /*
                                if (entity.HasComponent(index)) component = entity.GetComponent(index);

                                else
                                {
                                    component = TypeManager.CreateComponent(index);
                                    entity.AddComponent(index, component);
                                }
                                
                                Serializer.GetSerializer(TypeManager.Types[index]).Update(component, componentReader);
                            */
                            }
                        }

                        //while (entityReader.Position < entityReader.Length) entity.RemoveComponent(entityReader.ReadByte());
                    }
                }

                while (reader.Position < reader.Length)
                     _entities[reader.ReadUInt()].Destroy();
            }
        }

        /// <summary>
        /// Serializes all entities matching the specified filter
        /// </summary>
        /// <param name="filter">Filter</param>
        public byte[] Serialize(Filter filter)
        {
            Group group = InternalFilter(filter);

            byte[] data;
            using (BinaryDataWriter writer = new BinaryDataWriter())
            {
                foreach (Archetype archetype in group)
                {
                    byte[] indices = archetype.ComponentIDs;

                    for (int i = 0; i < archetype.EntityCount; i++)
                    {
                        Entity entity = archetype[i];

                        BinaryDataWriter entityWriter = writer.TryWriteNode(sizeof(uint));
                        foreach (byte index in indices)
                        {
                            CompositeBinarySerializer ser = Serializer.GetSerializer(TypeManager.Types[index]);
                            BinaryDataWriter componentWriter = entityWriter.TryWriteNode(sizeof(byte));
                            ser.Serialize(archetype.GetComponents(index)[i], componentWriter);
                            entityWriter.WriteByte(index);
                            componentWriter.PushNode();
                        }

                        writer.WriteUInt(entity.ID);
                        entityWriter.PushNode();
                    }
                }

                data = writer.GetData();
            }

            return data;
        }

        /// <summary>
        /// Serializes all objects matching the specified filter. Regarding baseline
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="baseline">Baseline</param>
        public byte[] Serialize(Filter filter, Baseline<uint> baseline)
        {
            Group group = InternalFilter(filter);

            byte[] data;
            using (BinaryDataWriter writer = new BinaryDataWriter())
            {
                List<uint> entitiesBaseKeys = new List<uint>(baseline.BaselineKeys);
                foreach (Archetype archetype in group)
                {
                    byte[] indices = archetype.ComponentIDs;
                    for (int i = 0; i < archetype.EntityCount; i++)
                    {
                        Entity entity = archetype[i];

                        uint entityId = entity.ID;

                        BinaryDataWriter entityWriter = writer.TryWriteNode(sizeof(uint));
                        Baseline<byte> entityBaseline = baseline.GetOrCreateBaseline<Baseline<byte>>(entityId, 0, out bool entIsNew);
                        List<byte> entityBaseKeys = new List<byte>(entityBaseline.BaselineKeys);

                        foreach (byte index in indices)
                        {
                            CompositeBinarySerializer ser = Serializer.GetSerializer(TypeManager.Types[index]);
                            BinaryDataWriter compWriter = entityWriter.TryWriteNode(sizeof(byte));
                            Baseline<byte> compBaseline = entityBaseline.GetOrCreateBaseline<Baseline<byte>>(index, ser.Count, out bool compIsNew);
                            ser.Serialize(archetype.GetComponents(index)[i], compWriter, compBaseline);

                            if (compWriter.Length > 0 || compIsNew)
                            {
                                entityWriter.WriteByte(index);
                                compWriter.PushNode();
                            }

                            entityBaseKeys.Remove(index);
                        }

                        if (entityBaseKeys.Count > 0)
                        {
                            entityWriter.WriteByte(byte.MaxValue);

                            foreach (byte key in entityBaseKeys)
                            {
                                entityWriter.WriteByte(key);
                                entityBaseline.DestroyBaseline(key);
                            }
                        }

                        if (entityWriter.Length > 0 || entIsNew)
                        {
                            writer.WriteUInt(entityId);
                            entityWriter.PushNode();
                        }

                        entitiesBaseKeys.Remove(entityId);
                    }
                }

                if (entitiesBaseKeys.Count > 0)
                {
                    writer.WriteUInt(uint.MaxValue);

                    foreach (uint key in entitiesBaseKeys)
                    {
                        writer.WriteUInt(key);
                        baseline.DestroyBaseline(key);
                    }
                }

                data = writer.GetData();
            }

            return data;
        }
    }
}
