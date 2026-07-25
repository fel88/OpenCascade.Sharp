using OCCPort.Common;
using System;

namespace TKMath
{
    //! In this class implemented variation of Particle Swarm Optimization (PSO) method.
    //! A. Ismael F. Vaz, L. N. Vicente 
    //! "A particle swarm pattern search method for bound constrained global optimization"
    //!
    //! Algorithm description:
    //! Init Section:
    //! At start of computation a number of "particles" are placed in the search space.
    //! Each particle is assigned a random velocity.
    //!
    //! Computational loop:
    //! The particles are moved in cycle, simulating some "social" behavior, so that new position of
    //! a particle on each step depends not only on its velocity and previous path, but also on the
    //! position of the best particle in the pool and best obtained position for current particle.
    //! The velocity of the particles is decreased on each step, so that convergence is guaranteed.
    //!
    //! Algorithm output:
    //! Best point in param space (position of the best particle) and value of objective function.
    //!
    //! Pros:
    //! One of the fastest algorithms.
    //! Work over functions with a lot local extremums.
    //! Does not require calculation of derivatives of the functional.
    //!
    //! Cons:
    //! Convergence to global minimum not proved, which is a typical drawback for all stochastic algorithms.
    //! The result depends on random number generator.
    //!
    //! Warning: PSO is effective to walk into optimum surrounding, not to get strict optimum.
    //! Run local optimization from pso output point.
    //! Warning: In PSO used fixed seed in RNG, so results are reproducible.

    public class math_PSO
    {
        /**
        * Constructor.
        *
        * @param theFunc defines the objective function. It should exist during all lifetime of class instance.
        * @param theLowBorder defines lower border of search space.
        * @param theUppBorder defines upper border of search space.
        * @param theSteps defines steps of regular grid, used for particle generation.
                          This parameter used to define stop condition (TerminalVelocity).
        * @param theNbParticles defines number of particles.
        * @param theNbIter defines maximum number of iterations.
        */
        public math_PSO(math_MultipleVarFunction theFunc,
                           math_Vector theLowBorder,
                           math_Vector theUppBorder,
                           math_Vector theSteps,
                           int theNbParticles = 32,
                           int theNbIter = 100)
        {
            myLowBorder = new(1, theFunc.NbVariables());
            myUppBorder = new(1, theFunc.NbVariables());
            mySteps = new(1, theFunc.NbVariables());
            myN = theFunc.NbVariables();
            myNbParticles = theNbParticles;
            myNbIter = theNbIter;
            myFunc = theFunc;

            myLowBorder = theLowBorder;
            myUppBorder = theUppBorder;
            mySteps = theSteps;
        }
        //! Perform computations, particles array is constructed inside of this function.
        public void Perform(math_Vector theSteps,
                             ref double theValue,
                              ref math_Vector theOutPnt,
                                int theNbIter = 100)
        {
            throw new NotImplementedException();
            // Initialization.
            math_Vector aMinUV=new(1, myN), aMaxUV=new(1, myN);
        //    aMinUV = myLowBorder + (myUppBorder - myLowBorder) / aBorderDivisor;
            //aMaxUV = myUppBorder - (myUppBorder - myLowBorder) / aBorderDivisor;
            myNbIter = theNbIter;
            mySteps = theSteps;

            //// To generate initial distribution it is necessary to have grid steps.
            //math_PSOParticlesPool aPool(myNbParticles, myN);

            //// Generate initial particles distribution.
            //Standard_Boolean isRegularGridFinished = Standard_False;
            //Standard_Real aCurrValue;
            //math_Vector aCurrPoint(1, myN);

            //PSO_Particle* aParticle = aPool.GetWorstParticle();
            //aCurrPoint = aMinUV;
            //do
            //{
            //    myFunc->Value(aCurrPoint, aCurrValue);

            //    if (aCurrValue < aParticle->Distance)
            //    {
            //        Standard_Integer aDimIdx;
            //        for (aDimIdx = 0; aDimIdx < myN; ++aDimIdx)
            //        {
            //            aParticle->Position[aDimIdx] = aCurrPoint(aDimIdx + 1);
            //            aParticle->BestPosition[aDimIdx] = aCurrPoint(aDimIdx + 1);
            //        }
            //        aParticle->Distance = aCurrValue;
            //        aParticle->BestDistance = aCurrValue;

            //        aParticle = aPool.GetWorstParticle();
            //    }

            //    // Step.
            //    aCurrPoint(1) += Max(mySteps(1), 1.0e-15); // Avoid too small step
            //    for (Standard_Integer aDimIdx = 1; aDimIdx < myN; ++aDimIdx)
            //    {
            //        if (aCurrPoint(aDimIdx) > aMaxUV(aDimIdx))
            //        {
            //            aCurrPoint(aDimIdx) = aMinUV(aDimIdx);
            //            aCurrPoint(aDimIdx + 1) += mySteps(aDimIdx + 1);
            //        }
            //        else
            //            break;
            //    }

            //    // Stop criteria.
            //    if (aCurrPoint(myN) > aMaxUV(myN))
            //        isRegularGridFinished = Standard_True;
            //}
            //while (!isRegularGridFinished);

            //performPSOWithGivenParticles(aPool, myNbParticles, theValue, theOutPnt, theNbIter);
        }
        math_MultipleVarFunction myFunc;

        math_Vector myLowBorder; // Lower border.
        math_Vector myUppBorder; // Upper border.
        math_Vector mySteps; // steps used in PSO algorithm.
        int myN; // Dimension count.
        int myNbParticles; // Particles number.
        int myNbIter;
    }
}
