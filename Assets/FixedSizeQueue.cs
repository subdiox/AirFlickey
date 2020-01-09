using System.Collections.Generic;
using System.Linq;

public class FixedSizeQueue<T> : Queue<T> {
    public Queue<T> queue;
    private int size = 30;

    public FixedSizeQueue() {
        queue = new Queue<T>();
    }

    public FixedSizeQueue(int size) {
        queue = new Queue<T>();
        this.size = size;
    }

    public new int Count() {
        return queue.Count();
    }

    public new T Peek() {
        return queue.Peek();
    }

    public new void Enqueue(T obj) {
        while (queue.Count() > size) {
            queue.Dequeue();
        }
        queue.Enqueue(obj);
    }
}