using System;
using GameTypes;

namespace TingTing
{
    public interface IInteractable
    {
        IntPoint[] interactionPoints {
            get;
        }
		
		string tooltipName
		{
			get;
		}
		
		string verbDescription
		{
			get;
		}

        bool CanTingInteractWithMe(Ting interacter);
        void OnInteracted(Ting pInteracter);
		
		/*
        Room Room
        {
            get;
        }
        */

    }
}
