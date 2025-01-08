using System.Collections.Generic;

namespace ChunkEditor;

public interface ICommandSuggester
{ 
    List<string> OnSuggest(Command command);
}