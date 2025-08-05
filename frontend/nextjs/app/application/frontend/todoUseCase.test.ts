import { todoUseCase } from './todoUseCase';


jest.mock('@infrastructure/todoRepository', () => ({
    todoRepository: {
        getAll: jest.fn(),
        create: jest.fn(),
        update: jest.fn(),
        delete: jest.fn(),
        getById: jest.fn(),
    },
}));

import { todoRepository } from '@infrastructure/todoRepository';
const getAllMock = todoRepository.getAll as jest.Mock;
const createMock = todoRepository.create as jest.Mock;
const updateMock = todoRepository.update as jest.Mock;
const deleteMock = todoRepository.delete as jest.Mock;
const getByIdMock = todoRepository.getById as jest.Mock;

describe('todoUseCase', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should call getAll from todoRepository', async () => {
        getAllMock.mockResolvedValue([{ id: '1', title: 'Test Todo' }]);
        const result = await todoUseCase.getAll();
        expect(getAllMock).toHaveBeenCalled();
        expect(result).toEqual([{ id: '1', title: 'Test Todo' }]);
    });

    it('should call create from todoRepository', async () => {
        createMock.mockResolvedValue({ id: '2', title: 'New Todo' });
        const result = await todoUseCase.create('New Todo');
        expect(createMock).toHaveBeenCalledWith('New Todo');
        expect(result).toEqual({ id: '2', title: 'New Todo' });
    });

    it('should call update from todoRepository', async () => {
        updateMock.mockResolvedValue({ id: '1', title: 'Updated Todo' });
        const result = await todoUseCase.update('1', 'Updated Todo');
        expect(updateMock).toHaveBeenCalledWith('1', 'Updated Todo');
        expect(result).toEqual({ id: '1', title: 'Updated Todo' });
    });

    it('should call delete from todoRepository', async () => {
        deleteMock.mockResolvedValue(undefined);
        await todoUseCase.delete('1');
        expect(deleteMock).toHaveBeenCalledWith('1');
    });

    it('should call getById from todoRepository', async () => {
        getByIdMock.mockResolvedValue({ id: '1', title: 'Test Todo' });
        const result = await todoUseCase.getById('1');
        expect(getByIdMock).toHaveBeenCalledWith('1');
        expect(result).toEqual({ id: '1', title: 'Test Todo' });
    });
});