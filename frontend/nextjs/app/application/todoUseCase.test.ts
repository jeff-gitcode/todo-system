import { todoUseCase } from './todoUseCase';


jest.mock('@infrastructure/services/todoService', () => ({
    todoService: {
        getAll: jest.fn(),
        create: jest.fn(),
        update: jest.fn(),
        delete: jest.fn(),
    },
}));

// Import the mocked todoService for assertions
import { todoService } from '@infrastructure/services/todoService';

describe('todoUseCase', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should call getAll from todoService', async () => {
        (todoUseCase.getAll as jest.Mock).mockReturnValue(Promise.resolve([{ id: '1', title: 'Test', completed: false }]));
        const result = await todoUseCase.getAll();
        expect(todoService.getAll).toHaveBeenCalled();
        expect(result).toEqual([{ id: '1', title: 'Test', completed: false }]);
    });

    it('should call create from todoService', async () => {
        (todoService.create as jest.Mock).mockResolvedValue({ id: '1', title: 'New Todo', completed: false });
        const result = await todoUseCase.create('New Todo');
        expect(todoService.create).toHaveBeenCalledWith('New Todo');
        expect(result).toEqual({ id: '1', title: 'New Todo', completed: false });
    });

    it('should call update from todoService', async () => {
        (todoService.update as jest.Mock).mockResolvedValue({ id: '1', title: 'Updated Todo', completed: false });
        const result = await todoUseCase.update('1', 'Updated Todo');
        expect(todoService.update).toHaveBeenCalledWith('1', 'Updated Todo');
        expect(result).toEqual({ id: '1', title: 'Updated Todo', completed: false });
    });

    it('should call delete from todoService', async () => {
        (todoService.delete as jest.Mock).mockResolvedValue(undefined);
        await todoUseCase.delete('1');
        expect(todoService.delete).toHaveBeenCalledWith('1');
    });
});