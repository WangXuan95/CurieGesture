#include <SerialFlash.h>
#include <CuriePME.h>
#include "SaveAndLoadNeurons.h"


static bool create_if_not_exists(const char *filename, uint32_t fileSize){
  if (!SerialFlash.exists(filename)) {
    return SerialFlash.createErasable(filename, fileSize);
  }
  return true;
}


void saveNetworkKnowledge(){
  SerialFlashFile file;

  Intel_PMT::neuronData neuronData;
  uint32_t fileSize = 128 * sizeof(neuronData);

  create_if_not_exists( FILE_NAME, fileSize );
  // Open the file and write test data
  file = SerialFlash.open(FILE_NAME);
  file.erase();

  CuriePME.beginSaveMode();
  if (file) {
    // iterate over the network and save the data.
    while( uint16_t nCount = CuriePME.iterateNeuronsToSave(neuronData)) {
      if( nCount == 0x7FFF)
        break;

      uint16_t neuronFields[4];

      neuronFields[0] = neuronData.context;
      neuronFields[1] = neuronData.influence;
      neuronFields[2] = neuronData.minInfluence;
      neuronFields[3] = neuronData.category;

      file.write( (void*) neuronFields, 8);
      file.write( (void*) neuronData.vector, 128 );
    }
  }

  CuriePME.endSaveMode();
}


bool restoreNetworkKnowledge(){
  SerialFlashFile file;
  int32_t fileNeuronCount = 0;

  Intel_PMT::neuronData neuronData;

  // Open the file and write test data
  file = SerialFlash.open(FILE_NAME);

  if (file) {
    CuriePME.beginRestoreMode();
    // iterate over the network and save the data.
    for(;;) {
      uint16_t neuronFields[4];
      file.read( (void*) neuronFields, 8);
      file.read( (void*) neuronData.vector, 128 );

      neuronData.context = neuronFields[0] ;
      neuronData.influence = neuronFields[1] ;
      neuronData.minInfluence = neuronFields[2] ;
      neuronData.category = neuronFields[3];

      if (neuronFields[0] == 0 || neuronFields[0] > 127)
        break;

      fileNeuronCount++;

      CuriePME.iterateNeuronsToRestore( neuronData );
    }
    CuriePME.endRestoreMode();
    return true;
  }else{
    return false;
  }

}
