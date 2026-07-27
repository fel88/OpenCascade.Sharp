namespace TKXSBASE
{
    public class yyFlexLexer
    {


        int yy_buffer_stack_top; /**< index of top of stack. */
        int yy_buffer_stack_max; /**< capacity of stack. */
        yy_buffer_state[] yy_buffer_stack; /**< Stack as an array. */


        /* We provide macros for accessing buffer states in case in the
 * future we want to put the buffer states in a more general
 * "scanner state".
 *
 * Returns the top of the stack, or NULL.
 */
        yy_buffer_state YY_CURRENT_BUFFER => (yy_buffer_stack != null)
                                  ? (yy_buffer_stack)[(yy_buffer_stack_top)]
                                  : null;

        Stream yyin;  // input source for default LexerInput
        Stream yyout; // output sink for default LexerOutput

        /** Delegate to the new version that takes an istream reference.
         * @param input_file A readable stream.
         * 
         * @note This function does not reset the start condition to @c INITIAL .
         */
        public void yyrestart(Stream input_file)
        {
            if (input_file == null)
            {
                input_file = yyin;
            }
            yyrestart2(input_file);
        }

        /* Allocates the stack if it does not exist.
         *  Guarantees space for at least one push.
         */
        public void yyensure_buffer_stack()
        {
            int num_to_alloc;

            if ((yy_buffer_stack) == null)
            {

                /* First allocation is just for 2 elements, since we don't know if this
                 * scanner will even need a stack. We use 2 instead of 1 to avoid an
                 * immediate realloc on the next call.
                 */
                num_to_alloc = 1; /* After all that talk, this was set to 1 anyways... */
                (yy_buffer_stack) = new yy_buffer_state[num_to_alloc];
                //	if ( ! (yy_buffer_stack) )
                //YY_FATAL_ERROR( "out of dynamic memory in yyensure_buffer_stack()" );

                //   memset((yy_buffer_stack), 0, num_to_alloc* sizeof(struct yy_buffer_state*));

                (yy_buffer_stack_max) = num_to_alloc;
                (yy_buffer_stack_top) = 0;
                return;
            }

            if ((yy_buffer_stack_top) >= ((yy_buffer_stack_max)) - 1)
            {

                /* Increase the buffer to prepare for a possible push. */
                int grow_size = 8 /* arbitrary grow size */;

                num_to_alloc = (yy_buffer_stack_max) + grow_size;
                (yy_buffer_stack) = new yy_buffer_state[num_to_alloc];
                //	if ( ! (yy_buffer_stack) )
                //		YY_FATAL_ERROR( "out of dynamic memory in yyensure_buffer_stack()" );

                //   /* zero only the new slots.*/
                //   memset((yy_buffer_stack) + (yy_buffer_stack_max), 0, grow_size* sizeof(struct yy_buffer_state*));
                (yy_buffer_stack_max) = num_to_alloc;
            }
        }

        const char YY_END_OF_BUFFER_CHAR = (char)0;

        /** Discard all buffered characters. On the next scan, YY_INPUT will be called.
 * @param b the buffer state to be flushed, usually @c YY_CURRENT_BUFFER.
 * 
 */
        public void yy_flush_buffer(yy_buffer_state b)
        {
            if (b == null)
                return;

            b.yy_n_chars = 0;

            /* We always need two end-of-buffer characters.  The first causes
             * a transition to the end-of-buffer state.  The second causes
             * a jam in that state.
             */
            b.yy_ch_buf[0] = YY_END_OF_BUFFER_CHAR;
            b.yy_ch_buf[1] = YY_END_OF_BUFFER_CHAR;

            //b.yy_buf_pos = &b.yy_ch_buf[0];

            //  b.yy_at_bol = 1;
            //  b.yy_buffer_status = YY_BUFFER_NEW;

            // if (b == YY_CURRENT_BUFFER)
            //   yy_load_buffer_state();
        }



        /* Initializes or reinitializes a buffer.
 * This function is sometimes called more than once on the same buffer,
 * such as during a yyrestart() or at EOF.
 */
        public void yy_init_buffer(yy_buffer_state b, Stream file)

        {
            int oerrno = errno;

            yy_flush_buffer(b);

            //b.yy_input_file = file.rdbuf();
            b.yy_input_file = file;
            b.yy_fill_buffer = 1;

            /* If b is the current buffer, then yy_init_buffer was _probably_
             * called from yyrestart() or through yy_get_next_buffer.
             * In that case, we don't want to reset the lineno or column.
             */
            if (b != YY_CURRENT_BUFFER)
            {
                b.yy_bs_lineno = 1;
                b.yy_bs_column = 0;
            }

            b.yy_is_interactive = 0;
            errno = oerrno;
        }
        /** Allocate and initialize an input buffer state.
         * @param file A readable stream.
         * @param size The character buffer size in bytes. When in doubt, use @c YY_BUF_SIZE.
         * 
         * @return the allocated buffer state.
         */
        yy_buffer_state yy_create_buffer(Stream file, int size)
        {
            yy_buffer_state b;

            b = new yy_buffer_state();
            //if ( ! b )
            //YY_FATAL_ERROR( "out of dynamic memory in yy_create_buffer()" );

            b.yy_buf_size = size;

            /* yy_ch_buf has to be 2 characters longer than the size given because
             * we need to put in 2 end-of-buffer characters.
             */
            b.yy_ch_buf = new char[b.yy_buf_size + 2];//(char*)yyalloc((yy_size_t)(b->yy_buf_size + 2));

            //if ( ! b->yy_ch_buf )
            //YY_FATAL_ERROR( "out of dynamic memory in yy_create_buffer()" );

            b.yy_is_our_buffer = 1;

            yy_init_buffer(b, file);

            return b;
        }
        const int YY_BUF_SIZE = 16384;

        static int errno;
        /** Immediately switch to a different input stream.
         * @param input_file A readable stream.
         * 
         * @note This function does not reset the start condition to @c INITIAL .
         */
        public void yyrestart2(Stream input_file)
        {

            if (YY_CURRENT_BUFFER == null)
            {
                yyensure_buffer_stack();
                YY_CURRENT_BUFFER_LVALUE_(
                 yy_create_buffer(yyin, YY_BUF_SIZE));
            }

            yy_init_buffer(YY_CURRENT_BUFFER, input_file);
            yy_load_buffer_state();
        }
        /* Same as previous macro, but useful when we know that the buffer stack is not
 * NULL or when we need an lvalue. For internal use only.
 */
        yy_buffer_state YY_CURRENT_BUFFER_LVALUE => (yy_buffer_stack)[(yy_buffer_stack_top)];
        yy_buffer_state YY_CURRENT_BUFFER_LVALUE_(yy_buffer_state val) => (yy_buffer_stack)[(yy_buffer_stack_top)]=val;

        // Number of characters read into yy_ch_buf.
        int yy_n_chars;

        // yy_hold_char holds the character lost when yytext is formed.
        char yy_hold_char;
        char[] yytext;

        // Points to current character in buffer.
        char[] yy_c_buf_p;
        void yy_load_buffer_state()
        {
            (yy_n_chars) = YY_CURRENT_BUFFER_LVALUE.yy_n_chars;
            //(yytext_ptr) = (yy_c_buf_p) = YY_CURRENT_BUFFER_LVALUE.yy_buf_pos;
            //yyin.rdbuf(YY_CURRENT_BUFFER_LVALUE.yy_input_file);
            //(yy_hold_char) = *(yy_c_buf_p);
        }

    }
}