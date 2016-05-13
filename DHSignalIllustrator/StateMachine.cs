using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHSignalIllustrator
{
    class StateMachine
    {
        State comState;  //current (customized) state of the port
        WaitingForHeaderOne wFHOne;
        WaitingForHeaderTwo wFHTwo;
        WaitingForData wFData;

        DataBuffer buffer;

        const int packageHeaderOne = 0x7E;
        const int packageHeaderTwo = 0x45;

        public StateMachine(DataBuffer recorder)
        {
            wFHOne = new WaitingForHeaderOne(this, recorder);
            wFHTwo = new WaitingForHeaderTwo(this);
            wFData = new WaitingForData(this);
            comState = wFHOne;

            this.buffer = recorder;

        }

        public State getWaitingForHeaderOneState()
        {
            return wFHOne;
        }

        public State getWaitingForHeaderTwoState()
        {
            return wFHTwo;
        }

        public State getWaitingForDataState()
        {
            return wFData;
        }

        public void setCurrentState(State s)
        {
            comState = s;
        }

        public void parseData(byte dataByte)
        {
            if (dataByte == packageHeaderOne)
            {
                comState.receivedHeaderOne();
            }
            else if (dataByte == packageHeaderTwo)
            {
                comState.receivedHeaderTwo();
            }
            else
            {
                comState.receivedData(buffer.add(dataByte));
            }
        }

        public void saveBuffer()
        {
            buffer.saveBuffer();
            buffer.resetBufferIndex();
            setCurrentState(wFHOne);
        }

        public void start()
        {
            buffer.openFile();
        }

        public void stop()
        {
            saveBuffer();
            buffer.closeFile();
        }

        public void setWinWidthAndFilterRange(int[] parameters)
        {
            buffer.setWinWidthAndFilterRange(parameters);

        }
    }

    interface State
    {
        void receivedHeaderOne();
        void receivedHeaderTwo();
        void receivedData(bool doRefresh);
    }

    class WaitingForHeaderOne : State
    {
        StateMachine machine;
        DataBuffer buffer;

        public WaitingForHeaderOne() { }
        public WaitingForHeaderOne(StateMachine machine, DataBuffer buffer)
        {
            this.machine = machine;
            this.buffer = buffer;
        }
        public void receivedHeaderOne()
        {
            machine.setCurrentState(machine.getWaitingForHeaderTwoState());
            buffer.resetColumnIndex();
        }
        public void receivedHeaderTwo() { }
        public void receivedData(bool doRefresh) { }
    }

    class WaitingForHeaderTwo : State
    {
        StateMachine machine;

        public WaitingForHeaderTwo() { }
        public WaitingForHeaderTwo(StateMachine machine) { this.machine = machine; }

        public void receivedHeaderOne() { }
        public void receivedHeaderTwo()
        {
            machine.setCurrentState(machine.getWaitingForDataState());
        }
        public void receivedData(bool doRefresh) { }

    }

    class WaitingForData : State
    {
        StateMachine machine;

        public WaitingForData() { }
        public WaitingForData(StateMachine machine) { this.machine = machine; }

        public void receivedHeaderOne()
        {
            machine.setCurrentState(machine.getWaitingForHeaderTwoState());
        }

        public void receivedHeaderTwo() { }

        public void receivedData(bool doRefresh)
        {
            if (doRefresh)
            {
                machine.setCurrentState(machine.getWaitingForHeaderOneState());
            }
        }
    }
}
