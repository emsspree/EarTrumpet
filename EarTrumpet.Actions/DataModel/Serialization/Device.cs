﻿using EarTrumpet.DataModel;
namespace EarTrumpet.Actions.DataModel.Serialization
{
    public class Device
    {
        public string Id { get; set; }

        public AudioDeviceKind Kind { get; set; }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }

        public bool Equals(Device other)
        {
            return other.Id == Id;
        }
    }
}
