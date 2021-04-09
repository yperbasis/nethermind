using System;

namespace Nethermind.Pipeline
{
    public interface IPipelineElement<T>
    {
       void SubscribeToData(T data); 
       Action<T> Emit { set; }
    }
}

// najpierw TIn na TOut

// test ->  na bazie tego:
// foreach (var element in _elements)
// {
//     output = await element.ProcessAsync(input);
//     if (!output.IsValid)
//     {
//         break;
//     }
//     // TIn = TOut (poprzedni element);
//     input = output.Data;
// }
//
// return output;


// dostanie danych
