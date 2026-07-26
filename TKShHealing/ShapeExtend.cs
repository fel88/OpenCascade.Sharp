
namespace TKShHealing
{
    //! This package provides general tools and data structures common
    //! for other packages in SHAPEWORKS and extending CAS.CADE
    //! structures.
    //! The following items are provided by this package:
    //! - enumeration Status used for coding status flags in methods
    //! inside the SHAPEWORKS
    //! - enumeration Parametrisation used for setting global parametrisation
    //! on the composite surface
    //! - class CompositeSurface representing a composite surface
    //! made of a grid of surface patches
    //! - class WireData representing a wire in the form of ordered
    //! list of edges
    //! - class MsgRegistrator for attaching messages to the objects
    //! - tools for exploring the shapes
    //! -       tools for creating       new shapes.
    public class ShapeExtend
    {
        public static bool DecodeStatus(int flag,
                       ShapeExtend_Status status)
        {
            if (status == ShapeExtend_Status.ShapeExtend_OK) return (flag == 0);
            return (((flag & ShapeExtend.EncodeStatus(status)) != 0) ? true : false);
        }

        public static int EncodeStatus(ShapeExtend_Status status)
        {
            switch (status)
            {
                case ShapeExtend_Status.ShapeExtend_OK: return 0x0000;
                case ShapeExtend_Status.ShapeExtend_DONE1: return 0x0001;
                case ShapeExtend_Status.ShapeExtend_DONE2: return 0x0002;
                case ShapeExtend_Status.ShapeExtend_DONE3: return 0x0004;
                case ShapeExtend_Status.ShapeExtend_DONE4: return 0x0008;
                case ShapeExtend_Status.ShapeExtend_DONE5: return 0x0010;
                case ShapeExtend_Status.ShapeExtend_DONE6: return 0x0020;
                case ShapeExtend_Status.ShapeExtend_DONE7: return 0x0040;
                case ShapeExtend_Status.ShapeExtend_DONE8: return 0x0080;
                case ShapeExtend_Status.ShapeExtend_DONE: return 0x00ff;
                case ShapeExtend_Status.ShapeExtend_FAIL1: return 0x0100;
                case ShapeExtend_Status.ShapeExtend_FAIL2: return 0x0200;
                case ShapeExtend_Status.ShapeExtend_FAIL3: return 0x0400;
                case ShapeExtend_Status.ShapeExtend_FAIL4: return 0x0800;
                case ShapeExtend_Status.ShapeExtend_FAIL5: return 0x1000;
                case ShapeExtend_Status.ShapeExtend_FAIL6: return 0x2000;
                case ShapeExtend_Status.ShapeExtend_FAIL7: return 0x4000;
                case ShapeExtend_Status.ShapeExtend_FAIL8: return 0x8000;
                case ShapeExtend_Status.ShapeExtend_FAIL: return 0xff00;
            }
            return 0;
        }

        static bool init = false;

        internal static void Init()
        {
            if (init) return;

            init = true;

            // load Message File for Shape Healing
            //if (!Message_MsgFile::HasMsg("ShapeFix.FixSmallSolid.MSG0"))
            //{
            //    if (!Message_MsgFile::LoadFromEnv("CSF_SHMessage", "SHAPE"))
            //    {
            //        Message_MsgFile::LoadFromString(SHMessage_SHAPE_us, sizeof(SHMessage_SHAPE_us) - 1);
            //    }
            //    if (!Message_MsgFile::HasMsg("ShapeFix.FixSmallSolid.MSG0"))
            //    {
            //        throw Standard_ProgramError("Critical Error - message resources for ShapeExtend are invalid or undefined!");
            //    }
            //}
        }
    }
}
