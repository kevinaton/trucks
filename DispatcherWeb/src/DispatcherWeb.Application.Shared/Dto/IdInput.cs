﻿namespace DispatcherWeb.Dto
{
    public class IdInput
    {
        public IdInput()
        {

        }

        public IdInput(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public static implicit operator int(IdInput idInput) => idInput.Id;

        public static implicit operator IdInput(int id) => new IdInput(id);
    }
}
