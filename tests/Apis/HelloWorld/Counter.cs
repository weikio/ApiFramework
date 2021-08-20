using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld
{
    public class Counter
    {
        private readonly CounterService _counterService;

        public Counter(CounterService counterService)
        {
            _counterService = counterService;
        }

        public int GetValue()
        {
            return _counterService.GetValue();
        }

        public int GetMultiplied(int factor)
        {
            return factor * _counterService.GetValue();
        }
    }

    public class CounterService
    {
        private int _value;

        public CounterService(int initialValue)
        {
            _value = initialValue;
        }

        public int GetValue()
        {
            return _value++;
        }

        public void Reset(int value)
        {
            _value = value;
        }
    }
}
