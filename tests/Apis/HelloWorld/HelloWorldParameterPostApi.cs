﻿namespace HelloWorld
{
    public class HelloWorldParameterPostApi
    {
        public CreatedResult Create(CreatedDto dto)
        {
            return new CreatedResult(){FirstName = dto.FirstName, Age = dto.Age};
        }
    }
}