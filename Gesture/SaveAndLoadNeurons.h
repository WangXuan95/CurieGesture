#ifndef _SAVE_AND_LOAD_NEURONS_H_
#define _SAVE_AND_LOAD_NEURONS_H_

#define FlashChipSelect  21
#define FILE_NAME        "NeurData.dat"

void saveNetworkKnowledge();
bool restoreNetworkKnowledge();

#endif
