import { Todo } from './models';

export interface ITodoRepository {
    getAll(): Promise<Todo[]>;
    create(title: string): Promise<Todo>;
    update(id: string, title: string): Promise<Todo>;
    delete(id: string): Promise<void>;
}