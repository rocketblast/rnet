using System;
using System.Collections.Generic;

namespace Rnet_Battlefield.Helpers
{
    public static class StringExtension
    {
        public static List<string> Wordify(this string command)
        {
            List<string> result = new List<string>();

            string fullWord = string.Empty;
            int quoteStack = 0;
            bool isEscaped = false;

            foreach (char input in command)
            {

                if (input == ' ')
                {
                    if (quoteStack == 0)
                    {
                        result.Add(fullWord);
                        fullWord = String.Empty;
                    }
                    else
                    {
                        fullWord += ' ';
                    }
                }
                else if (input == 'n' && isEscaped == true)
                {
                    fullWord += '\n';
                    isEscaped = false;
                }
                else if (input == 'r' && isEscaped == true)
                {
                    fullWord += '\r';
                    isEscaped = false;
                }
                else if (input == 't' && isEscaped == true)
                {
                    fullWord += '\t';
                    isEscaped = false;
                }
                else if (input == '"')
                {
                    if (isEscaped == false)
                    {
                        if (quoteStack == 0)
                        {
                            quoteStack++;
                        }
                        else
                        {
                            quoteStack--;
                        }
                    }
                    else
                    {
                        fullWord += '"';
                    }
                }
                else if (input == '\\')
                {
                    if (isEscaped == true)
                    {
                        fullWord += '\\';
                        isEscaped = false;
                    }
                    else
                    {
                        isEscaped = true;
                    }
                }
                else
                {
                    fullWord += input;
                    isEscaped = false;
                }
            }

            result.Add(fullWord);

            return result;
        }
    }
}
